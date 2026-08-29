param(
    [string]$LegacyRoot = "C:\Workspace All\Workspace-Ray\git\Apis\Apis\Trunk",
    [string]$CurrentRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$StrictRoutes
)

$ErrorActionPreference = "Stop"

$ProjectPairs = @(
    @{
        Name = "BackendApi"
        LegacyControllers = "CMS.BackendAPI\Controllers"
        CurrentControllers = "Ray.BackendApi\Controllers"
    },
    @{
        Name = "FrontendApi"
        LegacyControllers = "CMS.FrontendAPI\Controllers"
        CurrentControllers = "Ray.FrontendApi\Controllers"
    }
)

function Remove-Comments {
    param([string]$Text)

    $withoutBlock = [regex]::Replace($Text, "/\*.*?\*/", "", "Singleline")
    return [regex]::Replace($withoutBlock, "(?m)//.*$", "")
}

function Find-MatchingBrace {
    param(
        [string]$Text,
        [int]$OpenBraceIndex
    )

    $depth = 0
    $inString = $false
    $inChar = $false
    $escaped = $false

    for ($i = $OpenBraceIndex; $i -lt $Text.Length; $i++) {
        $ch = $Text[$i]

        if ($escaped) {
            $escaped = $false
            continue
        }

        if ($ch -eq "\") {
            if ($inString -or $inChar) {
                $escaped = $true
            }
            continue
        }

        if ($ch -eq '"' -and -not $inChar) {
            $inString = -not $inString
            continue
        }

        if ($ch -eq "'" -and -not $inString) {
            $inChar = -not $inChar
            continue
        }

        if ($inString -or $inChar) {
            continue
        }

        if ($ch -eq "{") {
            $depth++
        }
        elseif ($ch -eq "}") {
            $depth--
            if ($depth -eq 0) {
                return $i
            }
        }
    }

    return -1
}

function Get-MethodBlocks {
    param([string]$Text)

    $clean = Remove-Comments $Text
    $pattern = "(?ms)(?<attrs>(?:^\s*\[[^\r\n]+\]\s*)*)^\s*public\s+(?:async\s+)?(?:Task\s*<\s*)?(?:IHttpActionResult|IActionResult|ActionResult|JsonResult|HttpResponseMessage)(?:\s*>)?\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\("
    $matches = [regex]::Matches($clean, $pattern)
    $blocks = New-Object System.Collections.Generic.List[object]

    foreach ($match in $matches) {
        $openBrace = $clean.IndexOf("{", $match.Index + $match.Length)
        if ($openBrace -lt 0) {
            continue
        }

        $closeBrace = Find-MatchingBrace $clean $openBrace
        if ($closeBrace -lt 0) {
            continue
        }

        $block = $clean.Substring($match.Index, $closeBrace - $match.Index + 1)
        $blocks.Add([pscustomobject]@{
            Name = $match.Groups["name"].Value
            Attributes = $match.Groups["attrs"].Value
            Body = $block
        })
    }

    return $blocks
}

function Get-HttpVerbs {
    param(
        [string]$ActionName,
        [string]$Attributes
    )

    $verbs = New-Object System.Collections.Generic.List[string]
    if ($Attributes -match "HttpGet") { $verbs.Add("GET") }
    if ($Attributes -match "HttpPost") { $verbs.Add("POST") }
    if ($Attributes -match "HttpPut") { $verbs.Add("PUT") }
    if ($Attributes -match "HttpDelete") { $verbs.Add("DELETE") }
    if ($verbs.Count -gt 0) { return @($verbs) }
    if ($ActionName -match "^Get") { return @("GET") }
    if ($ActionName -match "^Post") { return @("POST") }
    if ($ActionName -match "^Put") { return @("PUT") }
    if ($ActionName -match "^Delete") { return @("DELETE") }
    return @("UNKNOWN")
}

function Get-RouteValues {
    param([string]$Attributes)

    $routes = New-Object System.Collections.Generic.List[string]
    foreach ($match in [regex]::Matches($Attributes, "Route(?:Prefix)?\s*\(\s*""(?<route>[^""]+)""\s*\)")) {
        $routes.Add($match.Groups["route"].Value.Trim("/"))
    }

    return @($routes)
}

function Get-ResponseShape {
    param([string]$Body)

    $hasCmsResponse = $Body -match "CMSResponse\s*\("
    $hasJson = $Body -match "return\s+Json\s*\("
    $hasOkWithPayload = $Body -match "(return\s+|=>\s*)Ok\s*\(\s*[^)]"
    $hasOkEmpty = $Body -match "return\s+Ok\s*\(\s*\)"
    $hasContent = $Body -match "(return\s+|=>\s*)Content\s*\("
    $hasStatus = $Body -match "return\s+(BadRequest|NotFound|Unauthorized|StatusCode|Created|NoContent)\s*\("
    $hasDelegation = $Body -match "return\s+[A-Za-z_][A-Za-z0-9_]*\s*\("
    $hasSignOut = $Body -match "return\s+SignOut\s*\("

    if ($hasCmsResponse -and ($hasJson -or ($hasOkWithPayload -and $Body -notmatch "Ok\s*\(\s*CMSResponse"))) {
        return "Mixed"
    }

    if ($hasCmsResponse) { return "WrappedCmsResponse" }
    if ($hasJson -or $hasOkWithPayload -or $hasContent) { return "DirectPayload" }
    if ($hasOkEmpty -and -not $hasStatus) { return "EmptyOk" }
    if ($hasSignOut) { return "EmptyOk" }
    if ($hasDelegation) { return "Delegated" }
    if ($hasStatus) { return "StatusOnly" }
    return "Other"
}

function Get-Contracts {
    param(
        [string]$Root,
        [string]$ControllersRelativePath,
        [string]$ProjectName,
        [string]$Side
    )

    $dir = Join-Path $Root $ControllersRelativePath
    if (-not (Test-Path $dir)) {
        throw "No existe el directorio de controllers: $dir"
    }

    $contracts = New-Object System.Collections.Generic.List[object]
    foreach ($file in Get-ChildItem $dir -Filter "*Controller.cs" -File) {
        if ($file.Name -in @("BaseApiController.cs", "BaseController.cs")) {
            continue
        }

        $text = Get-Content $file.FullName -Raw
        $controller = $file.BaseName -replace "Controller$", ""
        $controllerRoutes = @(Get-RouteValues $text | Where-Object { $_ -like "api/*" })

        foreach ($method in Get-MethodBlocks $text) {
            $actionRoutes = @(Get-RouteValues $method.Attributes | Where-Object { $_ -notlike "api/*" })
            $verbs = @(Get-HttpVerbs $method.Name $method.Attributes)
            $shape = Get-ResponseShape $method.Body
            $line = ($text.Substring(0, [Math]::Min($text.Length, $text.IndexOf($method.Name))).Split("`n").Count)

            foreach ($verb in $verbs) {
                $contracts.Add([pscustomobject]@{
                    Project = $ProjectName
                    Side = $Side
                    Controller = $controller
                    Action = $method.Name
                    Verb = $verb
                    ControllerRoutes = ($controllerRoutes -join "|")
                    ActionRoutes = ($actionRoutes -join "|")
                    ResponseShape = $shape
                    File = $file.FullName
                    Line = $line
                })
            }
        }
    }

    return $contracts
}

function Get-Key {
    param($Contract)
    return "$($Contract.Project)|$($Contract.Controller)|$($Contract.Action)|$($Contract.Verb)"
}

$legacyContracts = New-Object System.Collections.Generic.List[object]
$currentContracts = New-Object System.Collections.Generic.List[object]

foreach ($pair in $ProjectPairs) {
    $legacyContracts.AddRange((Get-Contracts $LegacyRoot $pair.LegacyControllers $pair.Name "Legacy"))
    $currentContracts.AddRange((Get-Contracts $CurrentRoot $pair.CurrentControllers $pair.Name "Current"))
}

$legacyByKey = @{}
foreach ($contract in $legacyContracts) {
    $legacyByKey[(Get-Key $contract)] = $contract
}

$currentByKey = @{}
foreach ($contract in $currentContracts) {
    $currentByKey[(Get-Key $contract)] = $contract
}

$sharedKeys = @($legacyByKey.Keys | Where-Object { $currentByKey.ContainsKey($_) } | Sort-Object)
$responseMismatches = New-Object System.Collections.Generic.List[object]
$routeMismatches = New-Object System.Collections.Generic.List[object]
$plainTextBadRequests = New-Object System.Collections.Generic.List[object]
$ignoredMapperProfiles = New-Object System.Collections.Generic.List[object]

foreach ($pair in $ProjectPairs) {
    $controllerDir = Join-Path $CurrentRoot $pair.CurrentControllers
    foreach ($file in Get-ChildItem $controllerDir -Filter "*.cs" -File -Recurse) {
        $relative = $file.FullName.Substring($CurrentRoot.Length).TrimStart("\", "/")
        if ($relative -like "*\ExceptionController\*") {
            continue
        }

        $content = Remove-Comments (Get-Content $file.FullName -Raw)
        $lines = $content -split "`r?`n"
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match '\bBadRequest\s*\(\s*(?:\$?"|ex\.Message|[^)]*\.Message\s*\))') {
                $plainTextBadRequests.Add([pscustomobject]@{
                    Project = $pair.Name
                    File = $file.FullName
                    Line = $i + 1
                    Code = $lines[$i].Trim()
                })
            }
        }
    }
}

$mapperDir = Join-Path $CurrentRoot "Ray.Managers\MapperProfiles"
if (Test-Path $mapperDir) {
    foreach ($file in Get-ChildItem $mapperDir -Filter "*.cs" -File) {
        $lines = Get-Content $file.FullName
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match "ForAllMembers\s*\(\s*x\s*=>\s*x\.Ignore\s*\(\s*\)\s*\)") {
                $ignoredMapperProfiles.Add([pscustomobject]@{
                    File = $file.FullName
                    Line = $i + 1
                    Code = $lines[$i].Trim()
                })
            }
        }
    }
}

foreach ($key in $sharedKeys) {
    $legacy = $legacyByKey[$key]
    $current = $currentByKey[$key]

    $isUnresolvedDelegation = $legacy.ResponseShape -eq "Delegated" -or $current.ResponseShape -eq "Delegated"
    $payloadShapes = @("DirectPayload", "WrappedCmsResponse", "Mixed")
    $isPayloadContract = $payloadShapes -contains $legacy.ResponseShape -or $payloadShapes -contains $current.ResponseShape
    if ($isPayloadContract -and -not $isUnresolvedDelegation -and $legacy.ResponseShape -ne $current.ResponseShape) {
        $responseMismatches.Add([pscustomobject]@{
            Project = $legacy.Project
            Controller = $legacy.Controller
            Action = $legacy.Action
            Verb = $legacy.Verb
            LegacyResponse = $legacy.ResponseShape
            CurrentResponse = $current.ResponseShape
            CurrentFile = $current.File
            CurrentLine = $current.Line
        })
    }

    if ($StrictRoutes -and $legacy.ActionRoutes -and -not $current.ActionRoutes.Contains($legacy.ActionRoutes)) {
        $routeMismatches.Add([pscustomobject]@{
            Project = $legacy.Project
            Controller = $legacy.Controller
            Action = $legacy.Action
            Verb = $legacy.Verb
            LegacyRoutes = $legacy.ActionRoutes
            CurrentRoutes = $current.ActionRoutes
            CurrentFile = $current.File
            CurrentLine = $current.Line
        })
    }
}

$missingInCurrent = @($legacyByKey.Keys | Where-Object { -not $currentByKey.ContainsKey($_) } | Sort-Object)
$extraInCurrent = @($currentByKey.Keys | Where-Object { -not $legacyByKey.ContainsKey($_) } | Sort-Object)

Write-Host "Legacy actions: $($legacyContracts.Count)"
Write-Host "Current actions: $($currentContracts.Count)"
Write-Host "Shared actions: $($sharedKeys.Count)"
Write-Host "Missing in current: $($missingInCurrent.Count)"
Write-Host "Extra in current: $($extraInCurrent.Count)"
Write-Host "Response shape mismatches: $($responseMismatches.Count)"
Write-Host "Plain text BadRequest results: $($plainTextBadRequests.Count)"
Write-Host "Global ignored mapper profiles: $($ignoredMapperProfiles.Count)"

if ($responseMismatches.Count -gt 0) {
    Write-Host ""
    Write-Host "Response shape mismatches:"
    $responseMismatches | Format-Table Project, Controller, Action, Verb, LegacyResponse, CurrentResponse, CurrentFile, CurrentLine -AutoSize
}

if ($StrictRoutes) {
    Write-Host "Route mismatches: $($routeMismatches.Count)"
    if ($routeMismatches.Count -gt 0) {
        Write-Host ""
        Write-Host "Route mismatches:"
        $routeMismatches | Format-Table Project, Controller, Action, Verb, LegacyRoutes, CurrentRoutes, CurrentFile, CurrentLine -AutoSize
    }
}

if ($plainTextBadRequests.Count -gt 0) {
    Write-Host ""
    Write-Host "Plain text BadRequest results:"
    $plainTextBadRequests | Format-Table Project, File, Line, Code -AutoSize
}

if ($ignoredMapperProfiles.Count -gt 0) {
    Write-Host ""
    Write-Host "Global ignored mapper profiles:"
    $ignoredMapperProfiles | Format-Table File, Line, Code -AutoSize
}

if ($missingInCurrent.Count -gt 0) {
    Write-Host ""
    Write-Host "Actions present only in legacy:"
    $missingInCurrent | Select-Object -First 50 | ForEach-Object { Write-Host "  $_" }
    if ($missingInCurrent.Count -gt 50) {
        Write-Host "  ... $($missingInCurrent.Count - 50) more"
    }
}

if ($extraInCurrent.Count -gt 0) {
    Write-Host ""
    Write-Host "Actions present only in current:"
    $extraInCurrent | Select-Object -First 50 | ForEach-Object { Write-Host "  $_" }
    if ($extraInCurrent.Count -gt 50) {
        Write-Host "  ... $($extraInCurrent.Count - 50) more"
    }
}

if ($responseMismatches.Count -gt 0 -or $plainTextBadRequests.Count -gt 0 -or $ignoredMapperProfiles.Count -gt 0 -or ($StrictRoutes -and $routeMismatches.Count -gt 0)) {
    exit 1
}

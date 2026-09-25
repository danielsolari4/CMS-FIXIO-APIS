using System.Collections.Generic;
using Ray.Dtos;
using Ray.Dtos.Components;

namespace Ray.Managers.Helpers
{
    public static class LayoutStructureHelper
    {
        public static IEnumerable<ComponentBaseDto> GetComponents(RegionDto region)
        {
            if (region?.Components != null)
            {
                foreach (var component in region.Components)
                    yield return component;
            }

            if (region?.Regions != null)
            {
                foreach (var subRegion in region.Regions)
                {
                    foreach (var component in GetComponents(subRegion))
                        yield return component;
                }
            }
        }
    }
}
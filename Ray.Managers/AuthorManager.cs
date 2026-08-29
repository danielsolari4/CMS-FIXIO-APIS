using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Core;
using Ray.Managers.MapperProfiles;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Configuration;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface IAuthorManager : IManager<AuthorDto>
    {
        Task<ICollection<AuthorDto>> GetAll(int? skip, int? take, bool includeNews = false, bool includeMedia = false);
        Task<AuthorDto> GetById(int id, bool includeNews = false, bool includeMedia = false);
    }

    public class AuthorManager : BaseManager, IAuthorManager
    {
        private readonly IAuthorRepository _repository;
        private readonly IMediaRepository _mediaRepository;
        private readonly AppSettings _appSettings;

        public AuthorManager(IAuthorRepository repository, IMediaRepository mediaRepository, AppSettings appSettings)
        {
            _repository = repository;
            _mediaRepository = mediaRepository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<AuthorProfile>();
                cfg.AddProfile<NewsProfile>();
                cfg.AddProfile<CreateMediaProfile>();
                cfg.AddProfile<KeywordProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<AuthorDto>> GetAll(int? skip, int? take, bool includeNews = false, bool includeMedia = false)
        {
            var authors = new List<AuthorDto>();

            var authorSet = await _repository.Get(a => !a.IsDeleted);

            if (skip.HasValue)
                authorSet = authorSet.OrderBy(a => a.Id).Skip(skip.Value);

            if (take.HasValue)
                authorSet = authorSet.Take(take.Value);

            foreach (var author in authorSet)
                authors.Add(MapToDto(author, includeNews, includeMedia));

            return authors;
        }

        public async Task<AuthorDto> GetById(int id, bool includeNews = false, bool includeMedia = false)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var author = await _repository.GetById(id);
            return author != null ? MapToDto(author, includeNews, includeMedia) : null;
        }

        public async Task<ICollection<AuthorDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(skip, take, false, false);
        }

        public async Task<AuthorDto> GetById(int id)
        {
            return await GetById(id, false, false);
        }

        public async Task<AuthorDto> Add(AuthorDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");
            var author = (await _repository.Get(a => a.Email.Equals(dto.Email))).FirstOrDefault();

            if (author == null)
            {
                dto.IsEnabled = true;
                var newAuthor = await _repository.Add(await MapFromDto(dto));
                await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);

                return MapToDto(newAuthor);
            }
            else
            {
                if (author.IsDeleted)
                {
                    author.IsDeleted = false;
                    author.IsEnabled = true;
                    author.FirstName = dto.FirstName;
                    author.LastName = dto.LastName;
                    await _repository.Update(author);
                    await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);

                    return MapToDto(author);
                }
                else
                {
                    throw new InUseException("email"); // We do not allow two Authors with the same Email.
                }
            }
        }

        public async Task Update(AuthorDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var author = await _repository.GetById(dto.Id);

            if (author == null)
                throw new EntityException("author");

            if ((await _repository.Get(a => a.Id != dto.Id && a.Email.Equals(dto.Email))).Any())
                throw new InUseException("email");

            await _repository.Update(await MapFromDto(dto, author));
            await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);
        }

        public async Task Delete(AuthorDto dto)
        {
            var author = await _repository.GetById(dto.Id);

            if (author == null || author.IsDeleted)
                throw new EntityException("author");

            author.IsEnabled = false;
            author.IsDeleted = true;
            author.CacheSolr = false;

            await _repository.Update(author);
            await SolrHelper.DataImport(SolrCore.AUTHOR, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        private AuthorDto MapToDto(Author author, bool includeNews = false, bool includeMedia = false)
        {
            var dto = _Mapper.Map<AuthorDto>(author);

            if (includeNews)
            {
                foreach (var news in author.NewsAuthors)
                {
                    dto.News.Add(_Mapper.Map<NewsDto>(news));
                }
            }

            if (includeMedia)
            {
                if (author.Media != null)
                {
                    dto.Media = _Mapper.Map<MediaDto>(author.Media);

                    if (!string.IsNullOrWhiteSpace(author.Media.SizesPaths))
                    {
                        var sizesPaths = JsonConvert.DeserializeObject<dynamic>(author.Media.SizesPaths);
                        if (sizesPaths != null)
                        {
                            dto.Media.Size1Path = _appSettings.Content.AdminDomain + sizesPaths.Size1Path;
                            dto.Media.Size2Path = _appSettings.Content.AdminDomain + sizesPaths.Size2Path;
                            dto.Media.Size3Path = _appSettings.Content.AdminDomain + sizesPaths.Size3Path;
                            dto.Media.Size4Path = _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                            dto.Media.Size5Path = _appSettings.Content.AdminDomain + sizesPaths.Size5Path;
                        }
                    }
                }
            }


            return dto;
        }

        private async Task<Author> MapFromDto(AuthorDto authorDto, Author author = null)
        {
            Author ret;

            if (author == null)
            {
                var newAuthor = _Mapper.Map<Author>(authorDto);

                if (authorDto.Media != null)
                {
                    var media = await _mediaRepository.GetById(authorDto.Media.Id);

                    if (media == null)
                        throw new ArgumentNullException("imagen");

                    newAuthor.MediaId = media.Id;
                }

                ret = newAuthor;
            }
            else
            {
                _Mapper.Map<AuthorDto, Author>(authorDto, author);

                if (authorDto.Media != null && authorDto.Media.Id != author.MediaId)
                {
                    var media = await _mediaRepository.GetById(authorDto.Media.Id);

                    //if (media == null)
                    //throw new ArgumentNullException("imagen");

                    if (media != null)
                        author.MediaId = media.Id;
                }

                ret = author;
            }

            ret.CacheSolr = false;

            return ret;
        }
    }
}

﻿using Database.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using ScieenceAPI.Models;
using ScieenceAPI.Models.ForClients;

namespace ScieenceAPI.Clients
{

    public class SemanticScholarClient
    {
        private readonly HttpClient _client;
        private static string? _baseUrl;

        public SemanticScholarClient(IConfiguration configuration)
        {
            _baseUrl = configuration["SemanticScholar:Url"];

            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }

        public async Task<Response> GetPublicationsByKeyword(string q, int[] year)
        {
            try
            {
                var response = await _client.GetAsync($"/graph/v1/paper/search?query={q}&year={year[0]}-{year[1]}&offset=1&limit=100&fields=title,url,abstract,year,isOpenAccess,fieldsOfStudy,publicationTypes,authors,externalIds,publicationDate");
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;

                var resultOfDes = JsonConvert.DeserializeObject<SemanticScholarByKeyword>(content);

                var result = new Response();

                foreach (var pub in resultOfDes.data)
                {
                    var newPub = new Publication
                    {
                        Language = "en",
                        Url = pub.url,
                        Title = pub.title,
                        PublicationDate = pub.publicationDate,
                        PublicationYear = pub.year,
                        Description = pub.Abstract,
                        Doi = pub.externalIds.DOI,
                    };
                    if(pub.authors != null)
                    {
                        newPub.Authors = string.Join("; ", pub.authors.ConvertAll(x => x.name));
                    }

                    if(pub.fieldsOfStudy != null)
                    {
                        newPub.Subjects = string.Join("; ", pub.fieldsOfStudy);
                    }

                    pub.publicationTypes ??= ["Article"];
                    newPub.PublicationType = pub.publicationTypes[0];
                    result.Records.Add(newPub);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Response();
            }
        }

        public async Task<Response> GetPublicationsByAuthor(string author, int[] year)
        {
            try
            {
                var response = await _client.GetAsync($"/graph/v1/author/search?query={author}&fields=papers.title,papers.url,papers.abstract,papers.year,papers.isOpenAccess,papers.fieldsOfStudy,papers.publicationTypes,papers.authors,papers.externalIds,papers.publicationDate&limit=200");
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;

                SemanticScholarByAuthor resultOfDes = JsonConvert.DeserializeObject<SemanticScholarByAuthor>(content);

                var result = new Response();
                foreach (var data in resultOfDes.data)
                {
                    foreach (var pub in data.papers)
                    {

                        var newPub = new Publication
                        {
                            Language = "en",
                            Url = pub.url,
                            Title = pub.title,
                            PublicationDate = pub.publicationDate,
                            PublicationYear = pub.year,
                            Description = pub.Abstract,
                            Doi = pub.externalIds.DOI,
                        };
                        if (pub.authors != null)
                        {
                            newPub.Authors = string.Join("; ", pub.authors.ConvertAll(x => x.name));
                        }

                        if (pub.fieldsOfStudy != null)
                        {
                            newPub.Subjects = string.Join("; ", pub.fieldsOfStudy);
                        }

                        pub.publicationTypes ??= ["Article"];
                        newPub.PublicationType = pub.publicationTypes[0];
                        if(newPub.PublicationYear > year[0] && newPub.PublicationYear < year[1])
                        {
                            result.Records.Add(newPub);
                        }
                    }
                }
                return result;
            }
            catch(Exception ex)
            {
                return new Response();
            }
        }
        //https://api.semanticscholar.org/graph/v1/paper/search?query=covid+vaccination&offset=100&limit=3&fields=title,url,abstract,year,isOpenAccess,fieldsOfStudy,publicationTypes,authors,externalIds
    }
}

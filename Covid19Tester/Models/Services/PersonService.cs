using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Covid19Tester.Models.Services
{
    public class PersonService : IPersonService
    {
        Container container;
        public PersonService(CosmosClient cosmosClient,
                            string dbName,
                            string containerName)
        {
            this.container = cosmosClient.GetContainer(dbName, containerName);
        }
        public async Task AddItemAsync(Person person)
        {
            await container.CreateItemAsync<Person>(person, new PartitionKey(person.CprNumber));
        }
        public async Task<Person> DeleteItemAsync(string id, string cprNumber)
        {
            try
            {
                ItemResponse<Person> response =
                   await container.DeleteItemAsync<Person>(id, new PartitionKey(cprNumber));
                return response.Resource;
            }
            catch (CosmosException e) 
            {                
                throw new IPersonService.PersonDeleteException(e.Message);
            }
        }

        public async Task<Person> GetItemAsync(string id, string cprNumber)
        {
            try
            {
                ItemResponse<Person> response =
                    await container.ReadItemAsync<Person>(id, new PartitionKey(cprNumber));
                return response.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
        public async Task<IEnumerable<Person>> GetItemsAsync(string queryString)
        {
            FeedIterator<Person> personIterator =
                container.GetItemQueryIterator<Person>(new QueryDefinition(queryString));
            List<Person> resultPersons = new List<Person>();
            while (personIterator.HasMoreResults)
            {
                FeedResponse<Person> persons = await personIterator.ReadNextAsync();
                resultPersons.AddRange(persons.ToList());
            }
            return resultPersons;
        }
        public async Task UpsertItemAsync(string cprNumber, Person person)
        {            
            await container.UpsertItemAsync<Person>(person, new PartitionKey(cprNumber));
        }
    }
}


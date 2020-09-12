using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covid19Tester.Models.Services
{
    public interface IPersonService
    {
        Task AddItemAsync(Person person);
        Task<Person> DeleteItemAsync(string id, string cprNumber);
        Task<Person> GetItemAsync(string id, string cprNumber);
        Task<IEnumerable<Person>> GetItemsAsync(string queryString);
        Task UpsertItemAsync(string cprNumber, Person person);
    
        public class PersonDeleteException : Exception
        {
            public PersonDeleteException(string msg) : base(msg) { }
        } 

    }
}
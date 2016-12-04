using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Contoso_Bank_Mike.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Contoso_Bank_Mike
{
    public class Database
    {

        private static Database instance;
        private MobileServiceClient client;
        private IMobileServiceTable<ContosoAccounts> ContosoAccounts;

        private Database()
        {
            this.client = new MobileServiceClient("http://contosobankapp.azurewebsites.net/");
            this.ContosoAccounts = this.client.GetTable<ContosoAccounts>();
        }



        public MobileServiceClient AzureClient
        {
            get { return client; }
        }



        public static Database DatabaseInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Database();
                }

                return instance;
            }
        }




        public async Task<List<ContosoAccounts>> GetUser(string username)
        {
            return await this.ContosoAccounts.Where(user => user.UserName == username).ToListAsync();
        }



        public async Task AddUser(ContosoAccounts user)
        {
            await this.ContosoAccounts.InsertAsync(user);
        }

        public async Task UpdateUser(ContosoAccounts user)
        {
  
            await this.ContosoAccounts.UpdateAsync(user);
        }
    }


}
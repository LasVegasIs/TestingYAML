using Crey.Contracts;
using Crey.Kernel;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web.Service2Service;
using IAM.Data;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class GDPRInfo
    {
        public int AccountId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool Newsletter { get; set; }
    }


    [EnableCors]
    [ApiController]
    public class MigrationHelperController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDb;
        private readonly CreyRestClient _creyRestClient;

        public MigrationHelperController(
            CreyRestClient creyRestClient, 
            ApplicationDbContext applicationDb)
        {
            _creyRestClient = creyRestClient;
            _applicationDb = applicationDb;
        }

        [HttpPost("/DOBMigrate")]
        public async Task Migrate(string key, int start, int count)
        {
            if(key != "whatever 1267z")
            {
                throw new Crey.Exceptions.AccessDeniedException("");
            }

            var ids = Enumerable.Range(start, count).ToList();
            var response = await _creyRestClient.CreateRequest(HttpMethod.Get, AuthenticationDefaults.SERVICE_NAME, "/gdpr/migrate")
                .AddS2SHeader()
                .SetContentJsonBody(ids)
                .SendAndParseAsync<List<GDPRInfo>>();

            // responded ids
            var rid = response.Select(x => x.AccountId).ToList();

            //update accountDB
            var accounts = await (from d in _applicationDb.Users
                                  where rid.Contains(d.AccountId)
                                  select d)
                     .ToListAsync();
            foreach (var a in accounts)
            {
                var data = response.Find(x => x.AccountId == a.AccountId);
                a.NewsletterSubscribed = data.Newsletter;
            }

            //update existing UserData
            var userDatas = await (from d in _applicationDb.UserDatas
                            where rid.Contains(d.AccountId)
                            select d)
                     .ToListAsync();
            foreach(var u in userDatas)
            {
                var data = response.Find(x => x.AccountId == u.AccountId);
                u.DateOfBirth = data.DateOfBirth;
            }
            

            var doneIds = userDatas.Select(x => x.AccountId).ToHashSet();
            var todoList = response.Where(x => !doneIds.Contains(x.AccountId));

            //create new UserData
            foreach (var t in todoList)                
            {
                await _applicationDb.UserDatas.AddAsync(new DBUserData
                {
                     AccountId = t.AccountId,   
                    DateOfBirth = t.DateOfBirth,
                });
            }

            await _applicationDb.SaveChangesAsync();

        }
    }
}
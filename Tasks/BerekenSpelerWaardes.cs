﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Tasks
{
    public class BerekenSpelerWaardes : IRecurringTask
    {
        public string Cron => "*/5 * * * *";
        public bool Enabled => true;
        public async Task ExecuteAsync()
        {
            using (var db = new ApplicationDbContext())
            {
                var beurzen = await db.Beurzen.ToListAsync();
                foreach (var applicationUser in db.Users.Include(x => x.Aandelen))
                {
                    var waarde = applicationUser.Geld + (from aandeelHouder in applicationUser.Aandelen
                                     let beurs = beurzen.FirstOrDefault(x => x.BeursId == aandeelHouder.BeursId)
                                     where beurs != null
                                     select aandeelHouder.Aantal * beurs.HuidigeWaarde).Sum();
                    applicationUser.Waarde = waarde;
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
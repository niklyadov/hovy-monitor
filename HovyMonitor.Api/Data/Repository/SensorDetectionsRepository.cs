using HovyMonitor.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HovyMonitor.Api.Data.Repository
{
    public class SensorDetectionsRepository
    {
        protected readonly ApplicationContext Context;
        public SensorDetectionsRepository(ApplicationContext context)
        {
            Context = context;
        }

        public async Task<List<SensorDetection>> GetAll() =>
            await Context.Set<SensorDetection>().ToListAsync();

        public async Task<SensorDetection> GetById(int id) =>
            await Context.Set<SensorDetection>().FindAsync(id);

        public async Task<bool> Contains(int id) =>
            await Context.Set<SensorDetection>().AnyAsync(x => x.Id == id);

        public async Task<SensorDetection> Add(SensorDetection entity)
        {
            var entry = Context.Set<SensorDetection>().Add(entity);
            await Context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<SensorDetection> Update(SensorDetection entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<SensorDetection> Delete(int id)
        {
            var entity = await Context.Set<SensorDetection>().FindAsync(id);

            if (entity != null)
            {
                Context.Set<SensorDetection>().Remove(entity);
                await Context.SaveChangesAsync();
            }

            return entity;
        }
    }
}

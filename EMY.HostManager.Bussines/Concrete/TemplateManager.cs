﻿using EMY.HostManager.Bussines.Abstract;
using EMY.HostManager.DataAccess.Abstract;
using EMY.HostManager.DataAccess.Concrete;
using EMY.HostManager.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMY.HostManager.Bussines.Concrete
{
    public class TemplateManager : AbstractTemplateService
    {

        private IAsyncRepository<Template> repository = null;
        public TemplateManager(DbContext context)
        {
            this.repository = new GenericRepository<Template>(context);
        }

        public override async Task Add(Template newTemplate, int adderRef)
        {
            await repository.Add(newTemplate, adderRef);
        }

        public override async Task Delete(int id, int DeleterRef)
        {
            await repository.Remove(id, DeleterRef);
        }

        public override async Task<IEnumerable<Template>> GetAll()
        {
            var result = (await repository.GetAll()).Where(obj => !obj.IsDeleted).ToList();
            return result;
        }

        public override async Task<Template> GetByTemplateName(string templateName)
        {
            var result = await repository.FirstOrDefault(o => o.TemplateName == templateName && !o.IsDeleted);
            return result;
        }

        public override async Task<Template> GetByTeplateID(int teplateID)
        {
            var result = await repository.GetByPrimaryKey(teplateID);
            return result;
        }

        public override async Task Update(Template template, int updaterRef)
        {
            await repository.Update(template, updaterRef);
        }
    }
}

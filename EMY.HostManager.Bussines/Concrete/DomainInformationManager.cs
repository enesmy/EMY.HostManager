using EMY.HostManager.Bussines.Abstract;
using EMY.HostManager.DataAccess.Abstract;
using EMY.HostManager.DataAccess.Concrete;
using EMY.HostManager.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace EMY.HostManager.Bussines.Concrete
{
    public class DomainInformationManager : AbstractDomainService
    {

        private IAsyncRepository<domainInformation> repository = null;
        public DomainInformationManager(DbContext context)
        {
            this.repository = new GenericRepository<domainInformation>(context);
        }
        public override async Task Add(domainInformation domainInformation, int adderRef)
        {
            await repository.Add(domainInformation, adderRef);
        }

        public override async Task Delete(domainInformation domainInformation, int deleterRef)
        {
            await repository.Remove(domainInformation.DomainInformationID, deleterRef);
        }

        public override async Task<domainInformation> GetDomainByName(string domainName)
        {
            var result = await repository.FirstOrDefault(o => o.DomainName == domainName && !o.IsDeleted);
            return result;
        }

        public override async Task<domainInformation> GetDomainInformationByDomainInformationID(int domainInformationID)
        {
            var result = await repository.GetByPrimaryKey(domainInformationID);
            return result;
        }

        public override async Task<IEnumerable<domainInformation>> GetDomainList()
        {
            var result = await repository.GetWhere(o => !o.IsDeleted);
            return result;
        }

        public override async Task Update(domainInformation domainInformation, int updaterRef)
        {
            await repository.Update(domainInformation, updaterRef);
        }
    }
}

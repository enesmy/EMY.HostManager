﻿using EMY.HostManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMY.HostManager.Bussines.Abstract
{
    public abstract class AbstractServerInformationService
    {
        public abstract Task<IEnumerable<ServerInformation>> GetServerList();
        public abstract Task<ServerInformation> GetServerInformationByServerInformationID(int ServerInformationID);
        public abstract Task Add(ServerInformation serverInformation, int adderRef);
        public abstract Task Update(ServerInformation serverInformation, int updaterRef);
        public abstract Task Delete(ServerInformation serverInformation, int deleterRef);
    }
}

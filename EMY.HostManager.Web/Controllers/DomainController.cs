using EMY.HostManager.Bussines;
using EMY.HostManager.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace EMY.HostManager.Web.Controllers
{
    [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme)]
    public class DomainController : Controller
    {
        HostManagerFactory factory;
        public DomainController()
        {
            this.factory = new HostManagerFactory();
        }
        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "DomainShow,DomainFull,AdminFull")]
        public async Task<IActionResult> Index()
        {
            var domains = await factory.Domains.GetDomainList();
            return View(domains);
        }



        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "DomainAdd,DomainFull,AdminFull")]
        public IActionResult Create()
        {
            ViewBag.Error = false;
            ViewBag.ErrorMessage = "";
            domainInformation newDomain = new domainInformation();
            return View("CreateOrUpdate", newDomain);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "DomainUp,DomainFull,AdminFull")]
        public async Task<IActionResult> Edit(int domainInformationID)
        {
            ViewBag.Error = false;
            ViewBag.ErrorMessage = "";
            domainInformation domain = await factory.Domains.GetDomainInformationByDomainInformationID(domainInformationID);
            if (domain == null)
                return NotFound();
            return View("CreateOrUpdate", domain);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "DomainShow,DomainFull,AdminFull")]
        public async Task<IActionResult> Details(int domainInformationID)
        {
            domainInformation domain = await factory.Domains.GetDomainInformationByDomainInformationID(domainInformationID);
            if (domain == null)
                return NotFound();
            return View(domain);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "DomainDel,DomainFull,AdminFull")]
        public async Task<IActionResult> Delete(int domainInformationID)
        {
            domainInformation domain = await factory.Domains.GetDomainInformationByDomainInformationID(domainInformationID);
            if (domain == null)
                return NotFound();
            return View(domain);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> Run(int domainInformationID)
        {
            domainInformation domain = await factory.Domains.GetDomainInformationByDomainInformationID(domainInformationID);
            if (domain == null)
                return NotFound();

            var templates = await factory.Templates.GetAll();
            var servers = await factory.ServerInformations.GetServerList();

            ViewBag.servers = new SelectList(servers, "ServerInformationID", "ServerName");
            ViewBag.Templates = new SelectList(templates, "TemplateID", "TemplateName");

            return View(domain);
        }



        [HttpPost]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> Upload(int domainInformationID, int templateID, int serverInformationID)
        {
            var domain = await factory.Domains.GetDomainInformationByDomainInformationID(domainInformationID);
            if (domain == null)
                return NotFound("Domain information not found in database!");

            var template = await factory.Templates.GetByTeplateID(templateID);
            if (template == null)
                return NotFound("Template information not found in database!");

            var server = await factory.ServerInformations.GetServerInformationByServerInformationID(serverInformationID);
            if (server == null)
                return NotFound("Server information not found in database!");

            SshSystem ssh = new SshSystem(server.ServerAdress, server.UserName, server.Password, server.Port);

            if (ssh.CheckConnection())
            {
                if (!ssh.UploadStringWithSCP(template.TemplateCode.Replace("{domain}", domain.DomainAdress), "/etc/nginx/sites-enabled/" + domain.DomainName + ".conf"))
                    return Unauthorized("Undefined error!");
                else
                {

                    return Ok("Ready!");
                }

            }
            else
            {
                return Unauthorized("Server information is not correct!");
            }
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> ServiceTest(int serverInformationID)
        {
            return await RunSSH(serverInformationID, "nginx -t");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> ServiceReload(int serverInformationID)
        {
            return await RunSSH(serverInformationID, "nginx -s reload");

        }

        private async Task<IActionResult> RunSSH(int serverInformationID, string sshCode)
        {
            var server = await factory.ServerInformations.GetServerInformationByServerInformationID(serverInformationID);
            if (server == null)
                return NotFound();
            SshSystem ssh = new SshSystem(server.ServerAdress, server.UserName, server.Password, server.Port);
            string outmessage = "";
            if (!ssh.RunSSHCode(sshCode, ref outmessage)) return Unauthorized(outmessage);
            return Ok(outmessage);
        }

        [HttpGet, ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "DomainDel,DomainFull,AdminFull")]
        public async Task<IActionResult> DeleteConfirmed(int domainInformationID)
        {
            domainInformation domain = await factory.Domains.GetDomainInformationByDomainInformationID(domainInformationID);
            if (domain == null)
                return NotFound();

            await factory.Domains.Delete(domain, int.Parse(User.Identity.Name));
            return Redirect("Index");

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(domainInformation domain)
        {
            ViewBag.Error = true;

            var DomainNameUniqeControl = await factory.Domains.GetDomainByName(domain.DomainName);

            if (domain.DomainInformationID == 0)
            {
                if (!(User.IsInRole("DomainAdd") || User.IsInRole("AdminFull") || User.IsInRole("DomainAll")))
                    return Unauthorized();

                if (DomainNameUniqeControl != null)
                {
                    ViewBag.ErrorMessage = "Domain name already exist in database!";
                    return View("CreateOrUpdate", domain);

                }
                else
                {
                    await factory.Domains.Add(domain, int.Parse(User.Identity.Name));
                }
            }
            else
            {
                if (!(User.IsInRole("DomainUp") || User.IsInRole("AdminFull") || User.IsInRole("DomainAll")))
                    return Unauthorized();
                if (DomainNameUniqeControl.DomainInformationID != domain.DomainInformationID)
                {
                    ViewBag.ErrorMessage = "Domain name already exist in database!";
                    return View("CreateOrUpdate", domain);
                }
                else
                {
                    var foundDomain = await factory.Domains.GetDomainInformationByDomainInformationID(domain.DomainInformationID);
                    foundDomain.DomainName = domain.DomainName;
                    foundDomain.DomainAdress = domain.DomainAdress;

                    await factory.Domains.Update(foundDomain, int.Parse(User.Identity.Name));
                }
            }
            return Redirect("Index");
        }
    }
}

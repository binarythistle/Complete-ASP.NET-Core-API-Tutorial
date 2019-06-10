using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using CommandAPI.Models;

namespace CommandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {

        private readonly CommandContext _context;
        private IHostingEnvironment _hostEnv;

        public CommandsController(CommandContext context, IHostingEnvironment hostEnv)
        {
            _context = context;
            _hostEnv = hostEnv;
        }

        //GET:      api/commands
        [HttpGet]
        public ActionResult<IEnumerable<Command>> GetCommandItems()
        {
            if (Response != null)
                Response.Headers.Add("Environment", _hostEnv.EnvironmentName);

            return _context.CommandItems;
        }

        //GET:      api/commands/Id
        [HttpGet("{id}")]
        public ActionResult<Command> GetCommandItem(int id)
        {
            var commandItem = _context.CommandItems.Find(id);

            if (commandItem == null)
                return NotFound();


            return commandItem;
        }

        //POST:     api/commands
        [HttpPost]
        public ActionResult<Command> PostCommandItem(Command command)
        {
            _context.CommandItems.Add(command);

            try
            {
                _context.SaveChanges();
            }
            catch
            {
                return BadRequest();
            }

            return CreatedAtAction("GetCommandItem", new Command { Id = command.Id }, command);
        }

        //PUT:     api/commands/n
        [HttpPut("{id}")]
        public ActionResult PutCommandItem(int id, Command command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            _context.Entry(command).State = EntityState.Modified;
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult<Command> DeleteCommandItem(int id)
        {
            var commandItem = _context.CommandItems.Find(id);

            if (commandItem == null)
                return NotFound();

            _context.CommandItems.Remove(commandItem);
            _context.SaveChanges();

            return commandItem;
        }


    }
}
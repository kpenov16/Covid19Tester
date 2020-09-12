using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Covid19Tester.Models;
using Covid19Tester.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Covid19Tester.Controllers
{
    public class PersonController : Controller
    {
        private readonly IPersonService personService;
        public PersonController(IPersonService personService)
        {
            this.personService = personService;
        }

        private bool isNumber(string str) {
            try{
                int.Parse(str);
                return true;
            }catch(Exception e){
                return false;
            }
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index(string searchString)
        {
            if (searchString == null || searchString.Length == 0 || !isNumber(searchString))
            {
                return View(new List<Person>());                
            }
            else
            {
                return View(await personService.GetItemsAsync("SELECT * FROM c WHERE c.cprnumber = \"" + searchString + "\""));

            }
        }

        //show Create view
        [ActionName("Create")]
        public IActionResult Create()
        {
            return View();
        }

        //post Create view
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("CprNumber,FirstName,LastName,TestResultat")] Person person)
        {
            if (ModelState.IsValid)
            {
                person.Id = Guid.NewGuid().ToString();
                await this.personService.AddItemAsync(person);
            }
            return RedirectToAction("Index");
            //return View(person);
        }

        //show Edit view
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id, string cprNumber)
        {
            if (cprNumber == null || cprNumber.Length == 0)
            {
                return BadRequest();
            }
            Person person = await this.personService.GetItemAsync(id, cprNumber);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        //post Edit view
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("Id,CprNumber,FirstName,LastName,TestResultat")] Person person)
        {
            if (ModelState.IsValid)
            {
                //person.Id = Guid.NewGuid().ToString();
                await this.personService.UpsertItemAsync(person.CprNumber, person);
                return RedirectToAction("Index");
            }
            return View(person);
        }


        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id, string cprNumber)
        {            
            if (cprNumber == null || cprNumber.Length == 0)
                return BadRequest();
            
            try
            {
                await this.personService.DeleteItemAsync(id, cprNumber);
                return RedirectToAction("Index");
            }
            catch (IPersonService.PersonDeleteException e)
            {
                //send err msg
                return RedirectToAction("Error");
            }
            catch (Exception e)
            {
                //send err msg
                return RedirectToAction("Error");
            }
            /*
            if (person == null)
            {
                return NotFound();
            }
            return View(person);*/
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("id")] string id, [Bind("cprNumber")] string cprNumber)
        {
            await this.personService.DeleteItemAsync(id, cprNumber);
            
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id, string cprNumber)
        {
            return View(await this.personService.GetItemAsync(id, cprNumber));
        }


    }
}

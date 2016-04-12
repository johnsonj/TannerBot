﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Tanner.Persistence;

namespace Tanner.Controllers
{
    public class UserContextController : Controller
    {
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // HACK: Not the best way to handle authentication, but I want to disable this
            // controller when it's deployed to Azure.
            if (ConfigurationManager.AppSettings.Get("DISABLE_EDIT") != null)
                filterContext.Result = new RedirectResult("~/");
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await DocumentDBRepository<UserContext>.GetItemsAsync(d => true);
            return View(items);
        }

        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind(Include = "Id,Name,PhoneNumber,GuestAllotted")] UserContext model)
        {
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<UserContext>.CreateItemAsync(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind(Include = "Id,Name,PhoneNumber,GuestAllotted")] UserContext user_context)
        {
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<UserContext>.UpdateItemAsync(user_context.Id, user_context);
                return RedirectToAction("Index");
            }

            return View(user_context);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserContext user_context = await DocumentDBRepository<UserContext>.GetItemAsync(id);
            if (user_context == null)
            {
                return HttpNotFound();
            }

            return View(user_context);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserContext item = await DocumentDBRepository<UserContext>.GetItemAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind(Include = "Id")] string id)
        {
            await DocumentDBRepository<UserContext>.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserContext item = await DocumentDBRepository<UserContext>.GetItemAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }
    }
}
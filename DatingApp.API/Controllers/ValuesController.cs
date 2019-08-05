﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController: ControllerBase {
        private readonly DataContext _ctx;
        public ValuesController(DataContext ctx) {
            _ctx = ctx;

        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetValues() {
            var values = await _ctx.Values.ToListAsync();
            return Ok(values);
        }
        
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetValues(int id) {
            var value = await _ctx.Values.FirstOrDefaultAsync(x => x.Id == id);
            return Ok(value);
        }


        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value) {}

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {}

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id) {}
    }
}

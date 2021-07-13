﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back_End.Contexts;
using System.Text.Json;
using Back_End.Models;
using Microsoft.Extensions.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Back_End.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class HostGroupController {
        private readonly ModelContext myContext;
        public HostGroupController(ModelContext modelContext) {
            myContext = modelContext;
        }
    }
}

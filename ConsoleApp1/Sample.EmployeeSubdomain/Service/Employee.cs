﻿using Microsoft.Extensions.Logging;
using Sample.EmployeeSubdomain.Service.DatabaseContext;
using Sample.EmployeeSubdomain.Service.Entities;
using Sample.EmployeeSubdomain.Service.Interfaces;
using Sample.EmployeeSubdomain.Service.Messages;
using Sample.Sdk;
using Sample.Sdk.Core;
using Sample.Sdk.Msg.Data;
using Sample.Sdk.Msg.Interfaces;
using Sample.Sdk.Persistance;
using Sample.Sdk.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.EmployeeSubdomain.Service
{
    /// <summary>
    /// Encapsulate employee requirements and use services to implement them. A persistance ignorance object.
    /// </summary>
    public class Employee : PersistenceObject<Employee, EmployeeContext, EmployeeEntity>, IEmployee
    {
        private ILogger<Employee> _logger;
        public Employee(ILoggerFactory loggerFactory,
            IEntityContext<EmployeeContext, EmployeeEntity> entityContext,
            IMessageBusSender messageSender) : base(loggerFactory.CreateLogger<Employee>(), entityContext, messageSender)
        {
            Guard.ThrowWhenNull(entityContext, loggerFactory, messageSender);
            _logger = loggerFactory.CreateLogger<Employee>();
        }
        public async Task<EmployeeEntity> CreateAndSave(string name, string email)
        {
            _employee = new EmployeeEntity { Name = name, Email = email };
            try
            {
                await Save(new EmployeeAdded()
                {
                    Key = _employee.Id.ToString(),
                    CorrelationId = _employee.Id.ToString(),
                    Content = System.Text.Json.JsonSerializer.Serialize(_employee)
                }, sendNotification: true);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Creating and saving raised an error: {} {}", e.Message, e.StackTrace);
            }
            return _employee;
        }
        public async Task<EmployeeEntity> GetEmployee(Guid id)
        {
            return await GetEntityById(id);
        }
        protected override EmployeeEntity GetInMemoryEntity() => _employee;
        protected override void AttachEntity(EmployeeEntity entity) => _employee = entity;
        private EmployeeEntity _employee;

    }
}

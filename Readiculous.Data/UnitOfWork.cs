﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Readiculous.Data.Interfaces;
using System;

namespace Readiculous.Data
{

    /// <summary>
    /// Unit of Work Implementation
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        /// <summary>
        /// Gets the database context
        /// </summary>
        public DbContext Database { get; private set; }

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="serviceContext">The service context.</param>
        public UnitOfWork(ReadiculousDbContext serviceContext)
        {
            Database = serviceContext;
        }

        /// <summary>
        /// Saves the changes to database
        /// </summary>
        public void SaveChanges()
        {
            Database.SaveChanges();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}

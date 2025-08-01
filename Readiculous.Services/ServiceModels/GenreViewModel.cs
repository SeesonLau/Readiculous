﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Readiculous.Services.ServiceModels
{
    public class GenreViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(20, ErrorMessage = "Name must not exceed 20 characters!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(300, ErrorMessage = "Description must not exceed 300 characters!")]
        public string Description { get; set; }

        public string GenreId { get; set; }
        //public List<BookViewModel> Books { get; set; } = new List<BookViewModel>();
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}

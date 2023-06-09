﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OnlineShop.Models
{
    public class Products
    {

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string SecondaryImage1 { get; set; }
        public string SecondaryImage2 { get; set; }
        public string SecondaryImage3 { get; set; }
        [Display(Name = "Product Color")]
        public string ProductColor { get; set; }
        [Required]
        [Display(Name = "Available")]
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        [Display(Name = "Product Type")]
        
        [Required]
        [Range(1, 100, ErrorMessage = "Select Valid Product Type")]
        public int ProductTypeId { get; set; }
        [ForeignKey("ProductTypeId")]
        public virtual ProductTypes ProductTypes { get; set; }
        [Display(Name = "Special Tag")]
        [Required]
        [Range(1, 100, ErrorMessage = "Select Valid Special Tag")] 
        public int SpecialTagId { get; set; }
        [ForeignKey("SpecialTagId")]
        public virtual SpecialTag SpecialTag { get; set; }
        [NotMapped]
        public string SpecialTagName { get; set; }
        [NotMapped]
        public string ProductTypeName { get; set; }

        [NotMapped]
        public IFormFile primaryimagefile { get; set; }
        [NotMapped]
        public IFormFile secondaryimage1file { get; set; }
        [NotMapped]
        public IFormFile secondaryimage2file { get; set; }
        [NotMapped]
        public IFormFile secondaryimage3file { get; set; }
        [NotMapped]
        public int ProductId { get; set; }
        [NotMapped]
        public int Rating { get; set; }
        [NotMapped]
        public string UserId { get; set; }
        


    }
}

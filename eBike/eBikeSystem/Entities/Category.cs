﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eBikeSystem.Entities;

public partial class Category
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(40)]
    [Unicode(false)]
    public string Description { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
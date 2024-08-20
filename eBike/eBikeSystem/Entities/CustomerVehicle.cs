﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eBikeSystem.Entities;

public partial class CustomerVehicle
{
    [Key]
    [StringLength(13)]
    public string VehicleIdentification { get; set; }

    [Required]
    [StringLength(20)]
    public string Make { get; set; }

    [Required]
    [StringLength(30)]
    public string Model { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerVehicles")]
    public virtual Customer Customer { get; set; }

    [InverseProperty("VehicleIdentificationNavigation")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
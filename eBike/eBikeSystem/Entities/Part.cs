﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eBikeSystem.Entities;

public partial class Part
{
    [Key]
    [Column("PartID")]
    public int PartId { get; set; }

    [Required]
    [StringLength(40)]
    [Unicode(false)]
    public string Description { get; set; }

    [Column(TypeName = "smallmoney")]
    public decimal PurchasePrice { get; set; }

    [Column(TypeName = "smallmoney")]
    public decimal SellingPrice { get; set; }

    public int QuantityOnHand { get; set; }

    public int ReorderLevel { get; set; }

    public int QuantityOnOrder { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(1)]
    [Unicode(false)]
    public string Refundable { get; set; }

    public bool Discontinued { get; set; }

    [Column("VendorID")]
    public int VendorId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Parts")]
    public virtual Category Category { get; set; }

    [InverseProperty("Part")]
    public virtual ICollection<JobDetailPart> JobDetailParts { get; set; } = new List<JobDetailPart>();

    [InverseProperty("Part")]
    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    [InverseProperty("Part")]
    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    [InverseProperty("Part")]
    public virtual ICollection<SaleRefundDetail> SaleRefundDetails { get; set; } = new List<SaleRefundDetail>();

    [ForeignKey("VendorId")]
    [InverseProperty("Parts")]
    public virtual Vendor Vendor { get; set; }
}
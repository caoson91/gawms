using AutoMapper;
using UniformWMS.Application.Features.Auth.DTOs;
using UniformWMS.Application.Features.Categories.DTOs;
using UniformWMS.Application.Features.Employees.DTOs;
using UniformWMS.Application.Features.IssuanceOrders.DTOs;
using UniformWMS.Application.Features.PurchaseOrders.DTOs;
using UniformWMS.Application.Features.ReturnOrders.DTOs;
using UniformWMS.Application.Features.Roles.DTOs;
using UniformWMS.Application.Features.Suppliers.DTOs;
using UniformWMS.Application.Features.UniformItems.DTOs;
using UniformWMS.Application.Features.Users.DTOs;
using UniformWMS.Domain.Entities;

namespace UniformWMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth / User
        CreateMap<AppUser, UserDto>()
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name)));
        CreateMap<AppRole, RoleDto>()
            .ForMember(d => d.Permissions, o => o.MapFrom(s => s.RolePermissions.Select(rp => rp.Permission.Code)));

        // Category
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryRequest, Category>();
        CreateMap<UpdateCategoryRequest, Category>();

        // Supplier
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierRequest, Supplier>();
        CreateMap<UpdateSupplierRequest, Supplier>();

        // Employee
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null));
        CreateMap<CreateEmployeeRequest, Employee>();
        CreateMap<UpdateEmployeeRequest, Employee>();

        // UniformItem
        CreateMap<UniformItem, UniformItemDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.SizeName, o => o.MapFrom(s => s.Size.ToString()));
        CreateMap<CreateUniformItemRequest, UniformItem>();
        CreateMap<UpdateUniformItemRequest, UniformItem>();

        // IssuanceOrder
        CreateMap<IssuanceOrder, IssuanceOrderDto>()
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.LineCount, o => o.MapFrom(s => s.Lines.Count));
        CreateMap<IssuanceOrderLine, IssuanceOrderLineDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.ItemName, o => o.MapFrom(s => s.Item != null ? s.Item.Name : null))
            .ForMember(d => d.ItemCode, o => o.MapFrom(s => s.Item != null ? s.Item.ItemCode : null));

        // ReturnOrder
        CreateMap<ReturnOrder, ReturnOrderDto>()
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.LineCount, o => o.MapFrom(s => s.Lines.Count));
        CreateMap<ReturnOrderLine, ReturnOrderLineDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.ItemName, o => o.MapFrom(s => s.Item != null ? s.Item.Name : null));

        // PurchaseOrder
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.ToString()));
        CreateMap<PurchaseOrderLine, PurchaseOrderLineDto>()
            .ForMember(d => d.ItemName, o => o.MapFrom(s => s.Item != null ? s.Item.Name : null))
            .ForMember(d => d.ItemCode, o => o.MapFrom(s => s.Item != null ? s.Item.ItemCode : null));
    }
}

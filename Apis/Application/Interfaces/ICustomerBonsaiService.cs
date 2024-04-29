﻿using Application.Commons;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CustomerBonsaiViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerBonsaiService
    {
        Task AddBonsaiForCustomer(CustomerBonsaiModel customerBonsaiModel, Guid customerId);
        Task CreateBonsai(Guid gardenId, BonsaiModelForCustomer bonsaiModelForCustomer);
        Task<Pagination<CustomerBonsai>> GetBonsaiOfGarden(Guid gardenId);
        Task<CustomerBonsaiViewModel> GetCustomerBonsaiById(Guid customerBonsaiId, Guid userId, bool isCustomer);
        Task MoveBonsai(Guid customerId, Guid customerBonsaiId, Guid customerGardenId);
        Task Update(Guid customerBonsaiId, BonsaiModelForCustomer bonsaiModelForCustomer);
        Task<Pagination<CustomerBonsaiViewModel>> GetBonsaiOfCustomer(Guid customerId, int pageIndex, int pageSize);
        Task CreateBonsaiWithNewGarden(Guid userId, BonsaiModelForCustomer bonsaiModelForCustomer);
        Task CreateNewGardenForBoughtBonsai(Guid userId, AddGardenForBoughtBonsai addGardenForBoughtBonsai);
    }
}

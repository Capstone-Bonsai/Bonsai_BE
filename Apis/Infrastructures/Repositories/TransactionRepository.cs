using Application;
using Application.Repositories;
using Application.Services;
using Application.ViewModels.DeliveryFeeViewModels;
using Application.ViewModels.OrderDetailModels;
using Application.ViewModels.OrderViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructures.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unit;

        public TransactionRepository(UserManager<ApplicationUser> userManager, AppDbContext context, IMapper mapper, IUnitOfWork unitOfWork, IDeliveryFeeService deliveryFeeService)
        {
            _context = context;
            _mapper = mapper;
            _unit = unitOfWork;
            _userManager = userManager;
            _deliveryFeeService = deliveryFeeService;
        }
        public async Task<Guid> CreateOrderByTransaction(OrderModel model, string? userId)
        {
            var customer = await GetCustomerAsync(model, userId);
            var myTransaction = _context.Database.BeginTransaction();
            try
            {
                Guid orderId = await CreateOrder(model, customer.Id);
                foreach (var item in model.ListProduct)
                {
                    await CreateOrderDetail(item, orderId);
                }
                await UpdateOrder(orderId);
                myTransaction.Commit();
                return orderId;
            }
            catch (Exception ex)
            {
                myTransaction.Rollback();
                throw new Exception(ex.Message);
            }
        }

        public async Task<IdentityResult> CreateUserAsync(OrderInfoModel model)
        {
            var user = new ApplicationUser
            {
                Email = model.Email,
                Fullname = model.Fullname,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Email,
                IsRegister = false
            };

            var result = await _userManager.CreateAsync(user);

            return result;
        }

        public async Task<Customer> GetCustomerAsync(OrderModel model, string? userId)
        {
            ApplicationUser? user = null;
            if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000"))
            {
                if (model.OrderInfo == null)
                    throw new Exception("Vui lòng thêm các thông tin người mua hàng.");
                user = await _userManager.FindByEmailAsync(model.OrderInfo.Email);
                if (user == null)
                {
                    var result = await CreateUserAsync(model.OrderInfo);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                    }
                    else
                    {
                        user = await _userManager.FindByEmailAsync(model.OrderInfo.Email);
                        //tạo account customer.
                        try
                        {
                            await _userManager.AddToRoleAsync(user, "Customer");
                            Customer cus = new Customer { UserId = user.Id };
                            await _unit.CustomerRepository.AddAsync(cus);
                            await _unit.SaveChangeAsync();
                        }
                        catch (Exception)
                        {
                            await _userManager.DeleteAsync(user);
                            throw new Exception("Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại!");
                        }
                    }
                }
            }
            else
            {
                user = await _userManager.FindByIdAsync(userId);
            }
            if (user == null)
                throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var customer = await _unit.CustomerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return customer;
        }

        public async Task<Guid> CreateOrder(OrderModel model, Guid customerId)
        {
            try
            {
                var order = _mapper.Map<Order>(model);
                order.DeliveryType = Domain.Enums.DeliveryType.PickupTruck;
                order.OrderDate = DateTime.Now;
                order.CustomerId = customerId;
                order.Price = 0;
                order.DeliveryPrice = 0;
                order.TotalPrice = 0;
                order.OrderStatus = Domain.Enums.OrderStatus.Waiting;
                order.OrderType = Domain.Enums.OrderType.Nomial;
                await _unit.OrderRepository.AddAsync(order);
                await _unit.SaveChangeAsync();
                return order.Id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateOrderDetail(OrderDetailModel model, Guid orderId)
        {
            try
            {
                var product = await _unit.ProductRepository.GetByIdAsync(model.ProductId);
                if (product == null)
                    throw new Exception("Không tìm thấy sản phẩm bạn muốn mua");
                else if ((product.Quantity - model.Quantity) < 0)
                    throw new Exception($"Số lượng sản phẩm {product.Name} trong kho không đủ số lượng bạn yêu cầu.");
                //tạo order đetail
                var orderDetail = _mapper.Map<OrderDetail>(model);
                orderDetail.OrderId = orderId;
                orderDetail.UnitPrice = product.UnitPrice;
                await _unit.OrderDetailRepository.AddAsync(orderDetail);

                //trừ quantity của product
                product.Quantity -= model.Quantity;
                _unit.ProductRepository.Update(product);
                await _unit.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task UpdateOrder(Guid orderId)
        {
            try
            {
                var order = await _unit.OrderRepository.GetAllQueryable().AsNoTracking().Where(x => x.Id == orderId && !x.IsDeleted).FirstOrDefaultAsync();
                if (order == null)
                    throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                List<Expression<Func<OrderDetail, object>>> includes = new List<Expression<Func<OrderDetail, object>>>{
                                 x => x.Product
                                    };
                var listOrderDetail = await _unit.OrderDetailRepository.GetAsync(expression: x => x.OrderId == orderId && !x.IsDeleted, isDisableTracking: true, isTakeAll: true, includes:includes);

                if (listOrderDetail == null || listOrderDetail.TotalItemsCount == 0)
                    throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                double total = 0;
                foreach (var item in listOrderDetail.Items)
                {
                    var temp = item.Quantity * item.Product.UnitPrice;
                    total += temp;
                }
                var deliveryPrice = await CalculateDeliveryPrice(order.Address, total);
                order.DeliveryType = deliveryPrice.deliveryFee.DeliveryType;
                order.DeliveryPrice = deliveryPrice.Price;
                order.Price = total;
                order.TotalPrice = total + deliveryPrice.Price;
                _context.ChangeTracker.Clear();
                _unit.OrderRepository.Update(order);
                await _unit.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<FeeViewModel> CalculateDeliveryPrice(string destination, double price)
        {
            var distance = await _deliveryFeeService.CalculateFee(destination, price);
            return distance;
        }
    }
}

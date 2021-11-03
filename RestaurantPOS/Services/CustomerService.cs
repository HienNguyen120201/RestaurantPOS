using Microsoft.AspNetCore.Identity;
using RestaurantPOS.Data;
using RestaurantPOS.Data.Entities;
using RestaurantPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantPOS.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly SignInManager<Customer> _signInManager;
        private readonly UserManager<Customer> _userManager;
        private readonly RestaurantDbContext _context;
        public CustomerService(
            SignInManager<Customer> signInManager,
            UserManager<Customer> userManager,
            RestaurantDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }
        public async Task<bool> LoginAsync(string username, string password)
        {
            var customer = await _userManager.FindByNameAsync(username);
            if (customer == null)
            {
                return false;
            }
            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            return result.Succeeded;
        }

        public async Task<bool> RegisterAsync(RegisterViewModel registerViewModel)
        {
            if (registerViewModel.Password != registerViewModel.RePassword) return false;

            var customer = await _userManager.FindByNameAsync(registerViewModel.UserName);
            if (customer != null)
            {
                return false;
            }

            var newCustomer = new Customer()
            {
                Id = System.Guid.NewGuid(),
                UserName = registerViewModel.UserName,
                PhoneNumber = registerViewModel.PhoneNumber,
                FullName = registerViewModel.FullName,
                Gender = registerViewModel.Gender,
                Birthday = registerViewModel.Birthday,
                VIP = false,
            };

            var result = await _userManager.CreateAsync(newCustomer, registerViewModel.Password);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        public async Task<List<TableHistoryViewModel>> GetTableHistoryAsync(ClaimsPrincipal user)
        {
            var customer = await _userManager.GetUserAsync(user);
            var tableOrderHistory = await (from f in _context.OderTable
                                           join g in _context.Table on f.TableId equals g.Id
                                           where f.CustomerId == customer.Id
                                           select new TableHistoryViewModel
                                           {
                                               Id = f.Id,
                                               From = f.From,
                                               To = f.To,
                                               TableName = g.Name,
                                               PeopleCount = g.PeopleCount,
                                           }).ToListAsync();
            return tableOrderHistory;
        }

        public async Task<PaymentViewModel> GetBillToPayAsync(ClaimsPrincipal user)
        {
            var customer = await _userManager.GetUserAsync(user);
            var foodPayment = await (from b in _context.Bill
                                     where b.CustomerId == customer.Id && b.PaymentMethod == string.Empty
                                     select b).FirstOrDefaultAsync();

            if (foodPayment == null)
            {
                return null;
            }

            var billToPay = await (from bd in _context.BillDetail
                                   join f in _context.Food on bd.FoodId equals f.Id
                                   where bd.BillId == foodPayment.Id
                                   select new BillViewModel
                                   {
                                       FoodName = f.Name,
                                       UnitPrice = f.UnitPrice,
                                       Quantity = bd.Quantity,
                                       Price = bd.Price
                                   }).ToListAsync();

            var payment = new PaymentViewModel
            {
                CreatedDate = foodPayment.CreatedDate,
                BillId = foodPayment.Id,
                BillToPay = billToPay,
                Total = foodPayment.Total
            };
            return payment;
        }

        public async Task UpdatePaymentMethodAsync(ClaimsPrincipal user, PaymentViewModel payment)
        {
            var customer = await _userManager.GetUserAsync(user);
            var billPayment = await (from b in _context.Bill
                                     where b.CustomerId == customer.Id
                                     select b).ToListAsync();

            // Update VIP
            if (billPayment.Count() > 10)
                customer.VIP = true;

            // Update payment method and total include VAT (10%)
            var update = (from u in billPayment
                          where u.PaymentMethod == string.Empty
                          select u).FirstOrDefault();
            update.PaymentMethod = payment.PaymentMethod;
            update.Total = update.Total * 11 / 10;

            _context.SaveChanges();
        }
    }
}
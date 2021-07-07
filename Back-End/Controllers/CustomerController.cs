﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back_End.Contexts;
using System.Text.Json;
using Back_End.Models;
using Microsoft.Extensions.Primitives;



//简单测试
namespace Back_End.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase {
        public class CustomerMessage
        {
            public int errorCode { get; set; }
            public Dictionary<string, dynamic> data { get; set; } = new Dictionary<string, dynamic>();
        }

        [HttpGet("createTime")]
        public string GetCreateTime()
        {
            StringValues token = default(StringValues);
            if (Request.Headers.TryGetValue("token", out token))
            {
                Console.WriteLine(token);
                var data = Token.VerifyToken(token);
                if (data != null)
                {
                    var context = ModelContext.Instance;
                    context.DetachAll();
                    int customerId = int.Parse(data["id"]);
                    var createTime = context.Customers.Single(b => b.CustomerId == customerId).CustomerCreatetime;
                    CustomerMessage message = new CustomerMessage()
                    {
                        errorCode = 200,
                        data = { { "createTime", createTime } }
                    };
                    return JsonSerializer.Serialize(message);
                }
            }
            return JsonSerializer.Serialize(new CustomerMessage()
            {
                errorCode = 400,
            });
        }



        public static bool CustomerLogin(Customer customer, string password)
        {
            try
            {
                if (customer == null)
                {
                    return false;
                }
                return customer.CustomerPassword == password;
            }
            catch
            {
                return false;
            }
        }


        [HttpPost("phone")]
        public string CheckCustomerPhoneRegisitered()
        {
            CheckPhoneMessage checkPhoneMessage = new CheckPhoneMessage();
            string phone = Request.Form["phonenumber"];
            string prePhone = Request.Form["prenumber"];
            if(phone!=null&&prePhone!=null)
            {
                checkPhoneMessage.errorCode = 200;
                if (SearchByPhone(phone, prePhone) == null)
                {
                    checkPhoneMessage.data["phoneunique"] = true;
                }
            }
            return checkPhoneMessage.ReturnJson();
        }



        public static Customer SearchByPhone(string phone, string prePhone)
        {
            try
            {
                var customer = ModelContext.Instance.Customers
                    .Single(b => b.CustomerPhone == phone && b.CustomerPrephone == prePhone);
                return customer;
            }
            catch
            {
                return null;
            }

        }
        public static Customer SearchById(int id)
        {
            try
            {
                var customer = ModelContext.Instance.Customers
                    .Single(b => b.CustomerId == id);
                return customer;
            }
            catch
            {
                return null;
            }

        }

        [HttpGet("details")]
        public string GetCustomerDetails()
        {
            CustomerDetailMessage customerDetailMessage = new CustomerDetailMessage();
            StringValues token = default(StringValues);
            if (Request.Headers.TryGetValue("token", out token))
            {
                customerDetailMessage.errorCode = 300;
                var data = Token.VerifyToken(token);
                if(data!=null)
                {
                    int id = int.Parse(data["id"]);
                    var customer = SearchById(id);
                    ICollection<Order> orders = customer.Orders;
                    List<HostComment> comments = new List<HostComment>();
                    foreach (var order in orders)
                    {
                        comments.Add(order.HostComment);
                    }
                    if (customer != null)
                    {
                        customerDetailMessage.errorCode = 200;
                        customerDetailMessage.data["userNickName"] = customer.CustomerName;
                        customerDetailMessage.data["userAvatar"] = customer.CustomerPhoto;
                        customerDetailMessage.data["evalNum"] = comments.Count;
                        customerDetailMessage.data["userGroupLevel"] = customer.CustomerLevel;
                        customerDetailMessage.data["emailTag"] = customer.CustomerEmail != null;
                        customerDetailMessage.data["userScore"] = customer.CustomerDegree;
                        customerDetailMessage.data["registerDate"] = customer.CustomerCreatetime;
                        customerDetailMessage.data["hostCommentList"] = comments;
                    }

                }
            }
            return customerDetailMessage.ReturnJson();
        }
        
        public string UploadPhoto(string base64)
        {
            //UNDONE:上传照片
            return null;
        }

        [HttpGet("avatar")]
        public string ChangeCustomerPhoto()
        {
            Message message = new Message();
            message.errorCode = 400;
            StringValues token = default(StringValues);
            if (Request.Headers.TryGetValue("token", out token))
            {
                message.errorCode = 300;
                var data = Token.VerifyToken(token);
                if(data!=null)
                {
                    int id = int.Parse(data["id"]);
                    var customer = SearchById(id);
                    string photo = Request.Query["avatarCode"];
                    Console.WriteLine(photo+"200");
                    if (photo != null)
                    {
                        try
                        {
                            string newPhoto = UploadPhoto(photo);
                            newPhoto = "/img/photo";
                            if (newPhoto != null)
                            {
                                Console.WriteLine(newPhoto);
                                customer.CustomerPhoto = newPhoto;
                                ModelContext.Instance.DetachAll();
                                ModelContext.Instance.SaveChanges();
                                message.errorCode = 200;
                            }
                        }
                        catch
                        {

                        }
                    }

                }
            }
            return message.ReturnJson();
        }

        [HttpGet("basicinfo")]
        public string ChangeCustomerInfo()
        {
            Message message = new Message();
            message.errorCode = 400;
            StringValues token = default(StringValues);
            if (Request.Headers.TryGetValue("token", out token))
            {
                message.errorCode = 300;
                var data = Token.VerifyToken(token);
                if(data!=null)
                {
                    int id = int.Parse(data["id"]);
                    var customer = SearchById(id);
                    string sex = Request.Query["userSex"];
                    DateTime birthday;
                    customer.CustomerName = Request.Query["userNickName"];
                    if (sex != null)
                    {
                        customer.CustomerGender = sex;
                    }
                    if (DateTime.TryParse(Request.Query["userBirthDate"], out birthday))
                    {
                        customer.CustomerBirthday = birthday;
                    }
                    try
                    {
                        message.errorCode = 200;
                        ModelContext.Instance.DetachAll();
                        ModelContext.Instance.SaveChanges();
                    }
                    catch
                    {

                    }

                }
            }
            return message.ReturnJson();
        }

        [HttpDelete]
        public bool Delete(int id)
        {
            if (id < 0)
                return false;
            try
            {
                var context = ModelContext.Instance;
                context.DetachAll();
                Customer customer = new Customer() { CustomerId = id };
                context.Customers.Attach(customer);
                context.Customers.Remove(customer);
                context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }




        /*
         *         [HttpGet("get")]
        public static Customer SearchById(int id)
        {
            try
            {
                var customer = ModelContext.Instance.Customers
                    .Single(b => b.CustomerId == id);
                return customer;
            }
            catch
            {
                return null;
            }

        }

        //GET: api/<Customer>
        public bool GetAction()
        {
            string action = Request.Form["action"];
            switch(action)
            {
                case "login":return Login();
                case "register":return Register();
                case "changepsw":return ChangePassword();
                default:return false;
            }
        }
        public bool Login()
        {
            string phone = Request.Form["phone"]; //接受 Form 提交的数据
            string email = Request.Form["mail"];
            string password = Request.Form["password"];
            string prePhone = Request.Form["prePhone"];
            Customer customer;
            if(email==null)
            {
                customer = SearchByPhone(phone, prePhone);
            }
            else
            {
                customer = SearchByEmail(email);
            }
            return CustomerLogin(customer, password);
        }

        public bool Register()
        {
            try
            {
                Customer customer = new Customer();
                customer.CustomerName = Request.Form["name"];
                customer.CustomerPassword = Request.Form["password"];
                customer.CustomerPrephone = Request.Form["prePhone"];
                customer.CustomerPhone = Request.Form["phone"];
                customer.CustomerCreatetime = DateTime.Now;
                ModelContext.Instance.Add(customer);
                ModelContext.Instance.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ChangePassword()
        {
            try
            {
                Customer customer = SearchById(int.Parse(Request.Form["id"]));
                if(customer==null)
                {
                    return false;
                }
                if(customer.CustomerPassword==Request.Form["password"])
                {
                    customer.CustomerPassword = Request.Form["newpassword"];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }



        public static Customer SearchByEmail(string email)
        {
            try
            {
                var customer = ModelContext.Instance.Customers
                    .Single(b => b.CustomerEmail == email);
                return customer;
            }
            catch
            {
                return null;
            }

        }
        
        [HttpGet]
        public static Customer SearchById(int id)
        {
            try
            {
                var customer = ModelContext.Instance.Customers
                    .Single(b => b.CustomerId == id);
                return customer;
            }
            catch
            {
                return null;
            }

        }
        */
    }

}

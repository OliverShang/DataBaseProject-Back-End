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

namespace Back_End.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ModelContext myContext;
        public OrderController(ModelContext modelContext)
        {
            myContext = modelContext;
        }

        public static Order SearchById(int id)
        {
            try
            {
                ModelContext context = new ModelContext();
                var order = context.Orders
                    .Single(b => b.OrderId == id);
                return order;
            }
            catch
            {
                return null;
            }

        }

        class OrderInfo
        {
            public int orderId { get; set; }
            public List<string> stayImage { get; set; }
            public int stayId { get; set; }
            public string stayName { get; set; }
            public string stayLocation { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
            public decimal totalCost { get; set; }
            public string name { get; set; }
            public string photo { get; set; }
            public int hostId { get; set; }
            public decimal commentStars { get; set; }
            public string comment { get; set; }
        }


        [HttpGet("CustomerOrderInfo")]
        public string GetCustomerOrder()
        {
            GetCustomerOrderMessage message = new GetCustomerOrderMessage();
            StringValues token = default(StringValues);
            if (Request.Headers.TryGetValue("token", out token))
            {
                message.errorCode = 300;
                var data = Token.VerifyToken(token);
                if (data != null)
                {
                    int id = int.Parse(data["id"]);
                    var customer = CustomerController.SearchById(id);
                    if(customer!=null)
                    {
                        var orders = customer.Orders.ToList();
                        List<OrderInfo> orderInfos = new List<OrderInfo>();
                        foreach(var order in orders)
                        {
                            OrderInfo orderInfo = new OrderInfo();
                            orderInfo.orderId = order.OrderId;
                            Stay stay = order.Generates.First().Room.Stay;
                            orderInfo.stayId = stay.StayId;
                            orderInfo.stayName = stay.StayName;
                            orderInfo.stayLocation = stay.DetailedAddress;
                            orderInfo.startTime = order.Generates.First().StartTime;
                            orderInfo.endTime = order.Generates.First().EndTime;
                            orderInfo.name = stay.Host.HostUsername;
                            orderInfo.totalCost = order.TotalCost;
                            orderInfo.hostId =(int) stay.HostId;
                            orderInfo.photo = stay.Host.HostAvatar;
                            List<string> photos = new List<string>();
                            foreach(var room in stay.Rooms)
                            {
                                foreach(var photo in room.RoomPhotos)
                                {
                                    photos.Add(photo.RPhoto);
                                }
                            }
                            orderInfo.stayImage = photos;
                            if(order.CustomerComment!=null)
                            {
                                orderInfo.commentStars = order.CustomerComment.HouseStars;
                                orderInfo.comment = order.CustomerComment.CustomerComment1;
                            }
                            else
                            {
                                orderInfo.commentStars = 0;
                                orderInfo.comment = null;
                            }
                            orderInfos.Add(orderInfo);
                        }
                        message.data["customerOrderList"] = orderInfos;
                        message.errorCode = 200;
                    }
                }
            }
            return message.ReturnJson();
        }

    }
}

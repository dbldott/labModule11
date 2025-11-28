
using System;
using System.Collections.Generic;
using System.Linq;




namespace OrderManagementSystem
{
    public interface IUserService
    {
        User Register(string name, string email, string password);
        User Authenticate(string email, string password);
        User GetById(Guid id);
    }

    public interface IProductService
    {
        Product AddProduct(string name, decimal price, string category, int stock);
        void UpdateProduct(Product product);
        IEnumerable<Product> GetAll();
        IEnumerable<Product> Find(string name, string category);
        void DecreaseStock(Product product, int quantity);
    }

    public interface IOrderService
    {
        Order CreateOrder(User user, IEnumerable<OrderItem> items);
        void CancelOrder(Guid orderId);
        Order GetById(Guid orderId);
    }

    public interface IPaymentService
    {
        PaymentResult ProcessPayment(Order order, PaymentDetails paymentDetails);
        PaymentStatus GetStatus(Guid paymentId);
    }

    public interface INotificationService
    {
        void NotifyOrderCreated(User user, Order order);
        void NotifyPaymentSuccess(User user, Order order);
        void NotifyOrderCancelled(User user, Order order);
    }

    public class User
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class Product
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int Stock { get; set; }
    }

    public class Order
    {
        public Guid Id { get; } = Guid.NewGuid();
        public User User { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class OrderItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }

    public class PaymentDetails
    {
        public string CardNumber { get; set; }
        public string HolderName { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }

    public class PaymentResult
    {
        public Guid PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UserService : IUserService
    {
        private readonly List<User> _users = new();

        public User Register(string name, string email, string password)
        {
            var user = new User { Name = name, Email = email };
            _users.Add(user);
            return user;
        }

        public User Authenticate(string email, string password)
        {
            return _users.Find(u => u.Email == email);
        }

        public User GetById(Guid id)
        {
            return _users.Find(u => u.Id == id);
        }
    }

    public class ProductService : IProductService
    {
        private readonly List<Product> _products = new();

        public Product AddProduct(string name, decimal price, string category, int stock)
        {
            var product = new Product { Name = name, Price = price, Category = category, Stock = stock };
            _products.Add(product);
            return product;
        }

        public void UpdateProduct(Product product)
        {
            // Stub: real implementation would persist changes
        }

        public IEnumerable<Product> GetAll() => _products;

        public IEnumerable<Product> Find(string name, string category)
        {
            foreach (var product in _products)
            {
                if ((string.IsNullOrWhiteSpace(name) ||
                     product.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(category) ||
                     product.Category.Equals(category, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return product;
                }
            }
        }

        public void DecreaseStock(Product product, int quantity)
        {
            product.Stock -= quantity;
        }
    }

    public class OrderService : IOrderService
    {
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly List<Order> _orders = new();

        public OrderService(IProductService productService, IPaymentService paymentService,
            INotificationService notificationService)
        {
            _productService = productService;
            _paymentService = paymentService;
            _notificationService = notificationService;
        }

        public Order CreateOrder(User user, IEnumerable<OrderItem> items)
        {
            var order = new Order { User = user, Items = new List<OrderItem>(items) };
            order.TotalAmount = 0m;

            foreach (var item in order.Items)
            {
                order.TotalAmount += item.Product.Price * item.Quantity;
                _productService.DecreaseStock(item.Product, item.Quantity);
            }

            order.Status = "Created";
            _orders.Add(order);

            _notificationService.NotifyOrderCreated(user, order);

            return order;
        }

        public void CancelOrder(Guid orderId)
        {
            var order = GetById(orderId);
            if (order == null) return;

            order.Status = "Cancelled";
            _notificationService.NotifyOrderCancelled(order.User, order);
        }

        public Order GetById(Guid orderId)
        {
            return _orders.Find(o => o.Id == orderId);
        }
    }

    public class PaymentService : IPaymentService
    {
        private readonly INotificationService _notificationService;
        private readonly Dictionary<Guid, PaymentResult> _payments = new();

        public PaymentService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public PaymentResult ProcessPayment(Order order, PaymentDetails paymentDetails)
        {
            var result = new PaymentResult
            {
                PaymentId = Guid.NewGuid(),
                Status = PaymentStatus.Success
            };

            _payments[result.PaymentId] = result;

            if (result.Status == PaymentStatus.Success)
            {
                order.Status = "Paid";
                _notificationService.NotifyPaymentSuccess(order.User, order);
            }

            return result;
        }

        public PaymentStatus GetStatus(Guid paymentId)
        {
            return _payments.TryGetValue(paymentId, out var result)
                ? result.Status
                : PaymentStatus.Failed;
        }
    }

    public class NotificationService : INotificationService
    {
        public void NotifyOrderCreated(User user, Order order)
        {
            Console.WriteLine($"[Email] Order {order.Id} created for {user.Email}");
        }

        public void NotifyPaymentSuccess(User user, Order order)
        {
            Console.WriteLine($"[Email] Payment success for order {order.Id} for {user.Email}");
        }

        public void NotifyOrderCancelled(User user, Order order)
        {
            Console.WriteLine($"[Email] Order {order.Id} cancelled for {user.Email}");
        }
    }
}

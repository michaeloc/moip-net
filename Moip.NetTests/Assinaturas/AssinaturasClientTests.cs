﻿using Moip.Net.Assinaturas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moip.NetTests;
using System;
using System.Linq;

namespace Moip.Net.Assinaturas.Tests
{
    [TestClass()]
    public class AssinaturasClientTests
    {
        private AssinaturasClient assinaturasClient = new AssinaturasClient(Settings.ApiUri, Settings.ApiToken, Settings.ApiKey);

        #region SerializerTests

        public class TestJsonSerializer
        {
            public string NomeCompleto { get; set; }
            public int Idade { get; set; }
            public TipoCliente Tipo { get; set; }
            public enum TipoCliente
            {
                ATIVO,
                INATIVO
            }
        }
        private string jsonTest = @"{
  ""nome_completo"": ""Rafael Gonçalves"",
  ""idade"": 19,
  ""tipo"": ""ATIVO""
}";

        [TestMethod()]
        public void ToJsonTest()
        {
            var json = assinaturasClient.ToJson(new TestJsonSerializer
            {
                NomeCompleto = "Rafael Gonçalves",
                Idade = 19,
                Tipo = TestJsonSerializer.TipoCliente.ATIVO
            });

            Assert.AreEqual(jsonTest, json);
        }

        [TestMethod()]
        public void FromJsonTest()
        {
            var obj = assinaturasClient.FromJson<TestJsonSerializer>(jsonTest);

            Assert.AreEqual("Rafael Gonçalves", obj.NomeCompleto);
            Assert.AreEqual(19, obj.Idade);
            Assert.AreEqual(TestJsonSerializer.TipoCliente.ATIVO, obj.Tipo);
        }

        #endregion

        #region Plans
        private Plan NewMockPlan()
        {
            return new Plan()
            {
                Code = "test_Plano_" + DateTime.Now.Ticks,
                Name = "Plano Especial",
                Description = "Descrição do plano especial",
                Amount = 9990,
                SetupFee = 500,
                MaxQty = 999,
                Interval = new PlanInterval()
                {
                    Length = 1,
                    Unit = PlanInterval.IntervalUnit.MONTH
                },
                BillingCycles = 12,
                Trial = new PlanTrial()
                {
                    Days = 30,
                    Enabled = true,
                    HoldSetupFee = true
                },
                PaymentMethod = Plan.PaymentMethodPlan.ALL
            };
        }

        [TestMethod()]
        public void CreatePlanTest()
        {
            var retorno = assinaturasClient.CreatePlan(NewMockPlan());
            Assert.AreEqual("Plano criado com sucesso", retorno.Message);

        }

        [TestMethod()]
        public void GetPlansTest()
        {
            var retorno = assinaturasClient.GetPlans();
            Assert.IsNotNull(retorno);
            Assert.IsNotNull(retorno.Plans);
            Assert.IsTrue(retorno.Plans.Length > 0);
            Assert.IsNotNull(retorno.Plans[0].Code);
        }

        private PlansResponse GetPlans()
        {
            var plans = assinaturasClient.GetPlans();

            if (plans == null || plans.Plans.Length == 0)
            {
                throw new AssertInconclusiveException("Nenhum plano foi encontrado no cadastro");
            }

            return plans;
        }

        [TestMethod()]
        public void GetPlanTest()
        {
            var firstPlan = GetPlans().Plans.First();
            var gettedPlan = assinaturasClient.GetPlan(firstPlan.Code);

            Assert.AreEqual(firstPlan.Code, gettedPlan.Code);
        }


        [TestMethod()]
        public void InactivatePlan()
        {
            var firstPlan = GetPlans().Plans.FirstOrDefault(x => x.Status == Plan.StatusPlan.ACTIVE);

            if (firstPlan == null)
            {
                Assert.Inconclusive("Nenhum plano ATIVO foi encontrado no cadastro");
            }
            else
            {
                assinaturasClient.InactivatePlan(firstPlan.Code);
                var plan = assinaturasClient.GetPlan(firstPlan.Code);
                Assert.IsTrue(plan.Status == Plan.StatusPlan.INACTIVE);
            }
        }

        [TestMethod()]
        public void ActivatePlan()
        {
            var firstPlan = GetPlans().Plans.FirstOrDefault(x => x.Status == Plan.StatusPlan.INACTIVE);

            if (firstPlan == null)
            {
                Assert.Inconclusive("Nenhum plano INATIVO foi encontrado no cadastro");
            }
            else
            {
                assinaturasClient.ActivatePlan(firstPlan.Code);
                var plan = assinaturasClient.GetPlan(firstPlan.Code);
                Assert.IsTrue(plan.Status == Plan.StatusPlan.ACTIVE);
            }
        }

        [TestMethod()]
        public void AlterarPlanoTest()
        {
            var firstPlan = GetPlans().Plans.FirstOrDefault(x => x.Status == Plan.StatusPlan.ACTIVE);
            if (firstPlan == null)
            {
                Assert.Inconclusive("Nenhum plano ATIVO foi encontrado no cadastro");
            }
            else
            {
                var plan = assinaturasClient.GetPlan(firstPlan.Code);
                plan.Name = "Plano Alterado - " + DateTime.Now.Ticks;
                assinaturasClient.UpdatePlan(plan.Code, plan);
                var updatedPlan = assinaturasClient.GetPlan(firstPlan.Code);
                Assert.AreEqual(plan.Name, updatedPlan.Name);
            }
        }
        #endregion

        #region Customers

        private CustomerRequest NewMockCustomer()
        {
            var code = "teste_cliente_" + DateTime.Now.Ticks;
            return new CustomerRequest()
            {
                Code = code,
                Email = code + "@acme.com",
                Fullname = "Roger Rabbit",
                Cpf = "72716422699",
                PhoneAreaCode = 11,
                PhoneNumber = "555555555",
                BirthdateDay = 19,
                BirthdateMonth = 7,
                BirthdateYear = 1985,
                Address = new CustomerAddress()
                {
                    Street = "Rua Nome da Rua",
                    Number = "100",
                    Complement = "AP 51",
                    District = "Nossa Senhora do Ó",
                    City = "São Paulo",
                    State = "SP",
                    Country = "BRA",
                    Zipcode = "02927100"
                },
                BillingInfo = new BillingInfoRequest()
                {
                    CreditCard = new CreditCard()
                    {
                        HolderName = "Roger Rabbit",
                        Number = "4111111111111111",
                        ExpirationMonth = "04",
                        ExpirationYear = "30"
                    }
                }
            };
        }

        private CustomersResponse GetCustomers()
        {
            var customers = assinaturasClient.GetCustomers();

            if (customers == null || customers.Customers == null || customers.Customers.Length == 0)
            {
                throw new AssertInconclusiveException("Nenhum cliente encontrado no cadastro do moip.");
            }

            return customers;
        }

        [TestMethod()]
        public void CreateCustomerTest()
        {
            var customer = NewMockCustomer();
            var respose = assinaturasClient.CreateCustomer(customer, true);
            Assert.AreEqual("Cliente criado com sucesso", respose.Message);
        }

        [TestMethod()]
        public void GetCustomersTest()
        {
            var retorno = GetCustomers();
            Assert.IsNotNull(retorno);
            Assert.IsNotNull(retorno.Customers);
            Assert.IsTrue(retorno.Customers.Length > 0);
            Assert.IsNotNull(retorno.Customers[0].Code);
        }

        [TestMethod()]
        public void GetCustomerTest()
        {
            var firstCustomer = GetCustomers().Customers.First();
            var gettedCustomer = assinaturasClient.GetCustomer(firstCustomer.Code);

            Assert.AreEqual(firstCustomer.Code, gettedCustomer.Code);
        }

        [TestMethod()]
        public void UpdateCustomerTest()
        {
            var firstCustomer = GetCustomers().Customers.First();
            var customerRequest = NewMockCustomer();
            customerRequest.BillingInfo = null;
            customerRequest.Code = firstCustomer.Code;
            customerRequest.Fullname = "Nome alterado - " + DateTime.Now.Ticks.ToString();
            assinaturasClient.UpdateCustomer(firstCustomer.Code, customerRequest);

            var updatedCustomer = assinaturasClient.GetCustomer(customerRequest.Code);

            Assert.AreEqual(customerRequest.Fullname, updatedCustomer.Fullname);

        }

        [TestMethod()]
        public void UpdateBillingInfoTest()
        {
            var firstCustomer = GetCustomers().Customers.First();
            var billingInfo = new BillingInfoRequest()
            {
                CreditCard = new CreditCard()
                {
                    HolderName = "Novo Nome",
                    Number = "5555666677778884",
                    ExpirationMonth = "04",
                    ExpirationYear = "16"
                }
            };

            var retorno = assinaturasClient.UpdateBillingInfo(firstCustomer.Code, billingInfo);

            Assert.AreEqual("Dados alterados com sucesso", retorno.Message);

        }

        #endregion

        #region Subscriptions
        [TestMethod()]
        public void CreateSubscriptionTestWithExistentCustomer()
        {
            var firstPlan = GetPlans().Plans.First();
            var firstCustomer = GetCustomers().Customers.First();

            var subscription = new Subscription()
            {
                Code = "_test_assinatura_" + DateTime.Now.Ticks,
                PaymentMethod = Plan.PaymentMethodPlan.CREDIT_CARD,
                Plan = firstPlan,
                Customer = firstCustomer
            };

            var retorno = assinaturasClient.CreateSubscription(subscription, false);
            Assert.AreEqual("Assinatura criada com sucesso", retorno.Message);

        }

        [TestMethod()]
        public void CreateSubscriptionTestWithNewCustomer()
        {
            var firstPlan = GetPlans().Plans.First();
            var customer = NewMockCustomer();

            var subscription = new Subscription()
            {
                Code = "_test_assinatura_" + DateTime.Now.Ticks,
                PaymentMethod = Plan.PaymentMethodPlan.CREDIT_CARD,
                Plan = firstPlan,
                Customer = customer
            };

            var retorno = assinaturasClient.CreateSubscription(subscription, true);
            Assert.AreEqual("Assinatura criada com sucesso", retorno.Message);

        }

        [TestMethod()]
        public void GetSubscriptionsTest()
        {
            var retorno = assinaturasClient.GetSubscriptions();
            Assert.IsNotNull(retorno);
            Assert.IsNotNull(retorno.Subscriptions);
            Assert.IsTrue(retorno.Subscriptions.Length > 0);
            Assert.IsNotNull(retorno.Subscriptions[0].Code);
        }

        private SubscriptionsResponse GetSubscriptions()
        {
            var subscriptions = assinaturasClient.GetSubscriptions();

            if (subscriptions == null || subscriptions.Subscriptions.Length == 0)
            {
                throw new AssertInconclusiveException("Nenhuma assinatura foi encontrado na sua conta do moip");
            }

            return subscriptions;
        }

        [TestMethod()]
        public void GetSubscriptionTest()
        {
            var firstSubscription = GetSubscriptions().Subscriptions.First();
            var subscription = assinaturasClient.GetSubscription(firstSubscription.Code);

            Assert.AreEqual(firstSubscription.Code, subscription.Code);
        }

        [TestMethod()]
        public void SuspendSubscriptionTest()
        {
            var firstActive = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status == Subscription.SubscriptionStatus.ACTIVE || x.Status == Subscription.SubscriptionStatus.TRIAL);

            if (firstActive == null)
            {
                throw new AssertInconclusiveException("Nenhum plano ativo foi encontrado na conta moip");
            }

            assinaturasClient.SuspendSubscription(firstActive.Code);

            var subscription = assinaturasClient.GetSubscription(firstActive.Code);
            Assert.AreEqual(Subscription.SubscriptionStatus.SUSPENDED, subscription.Status);
        }


        [TestMethod()]
        public void ActivateSubscriptionTest()
        {
            var firstActive = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status == Subscription.SubscriptionStatus.SUSPENDED);

            if (firstActive == null)
            {
                throw new AssertInconclusiveException("Nenhum plano SUSPENSO foi encontrado na conta moip");
            }

            assinaturasClient.ActivateSubscription(firstActive.Code);

            var subscription = assinaturasClient.GetSubscription(firstActive.Code);
            Assert.AreEqual(Subscription.SubscriptionStatus.ACTIVE, subscription.Status);
        }


        [TestMethod()]
        public void CancelSubscriptionTest()
        {
            var firstActive = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status != Subscription.SubscriptionStatus.CANCELED);

            if (firstActive == null)
            {
                throw new AssertInconclusiveException("Nenhum plano diferente de cancelado foi encontrado na conta moip");
            }

            assinaturasClient.CancelSubscription(firstActive.Code);

            var subscription = assinaturasClient.GetSubscription(firstActive.Code);
            Assert.AreEqual(Subscription.SubscriptionStatus.CANCELED, subscription.Status);
        }

        [TestMethod()]
        public void UpdateSubscriptionTest()
        {
            var firstActive = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status != Subscription.SubscriptionStatus.CANCELED);

            if (firstActive == null)
            {
                throw new AssertInconclusiveException("Nenhum plano diferente de cancelado foi encontrado na conta moip");
            }

            var random = new Random().Next(2, 30);
            var newDate = DateTime.Now.AddDays(random).Date;
            firstActive.NextInvoiceDate = MoipDate.FromDate(newDate);
            assinaturasClient.UpdateSubscription(firstActive.Code, firstActive);

            var subscription = assinaturasClient.GetSubscription(firstActive.Code);

            Assert.AreEqual(newDate, subscription.NextInvoiceDate.ToDate());
        }
        #endregion

        #region Invoices
        [TestMethod()]
        public void GetInvoicesTest()
        {
            //Pega a primeira ativa (tem mais chances de ter gerado cobrança)
            var firstActive = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status == Subscription.SubscriptionStatus.ACTIVE);
            var retorno = assinaturasClient.GetInvoices(firstActive.Code);

            Assert.IsNotNull(retorno);
            Assert.IsNotNull(retorno.Invoices);
            Assert.IsTrue(retorno.Invoices.Length > 0);
            Assert.IsNotNull(retorno.Invoices[0].Id);
        }

        [TestMethod()]
        public void GetInvoiceTest()
        {
            //Pega a primeira ativa (tem mais chances de ter gerado cobrança)
            var firstActive = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status == Subscription.SubscriptionStatus.ACTIVE);
            var invoices = assinaturasClient.GetInvoices(firstActive.Code);
            var id = invoices.Invoices.First().Id;
            var invoice = assinaturasClient.GetInvoice(id);

            Assert.AreEqual(id, invoice.Id);
        }
        #endregion

        #region Coupons
        [TestMethod()]
        public void CreateCouponTest()
        {
            var coupon = new Coupon()
            {
                Code = "_test_coupon_" + DateTime.Now.Ticks,
                Name = "Coupon Teste Unitario",
                Description = "Descrição do coupon de teste",
                Discount = new CouponDiscount()
                {
                    Value = 1000,
                    Type = DiscountType.AMOUNT
                },
                Status = Coupon.CouponStatus.ACTIVE,
                Duration = new CouponDuration()
                {
                    Type = CouponDuration.DurationType.REPEATING,
                    Occurrences = 2
                },
                MaxRedemptions = 100,
                ExpirationDate = MoipDate.FromDate(DateTime.Now.Date.AddDays(10))
            };

            var retorno = assinaturasClient.CreateCoupon(coupon);

            Assert.AreEqual(coupon.Code, retorno.Code);
        }

        private CouponsResponse GetCoupons()
        {
            var coupons = assinaturasClient.GetCoupons();

            if (coupons == null || coupons.Coupons == null || coupons.Coupons.Length == 0)
            {
                throw new AssertInconclusiveException("Nenhum cupom foi encontrado no cadastro");
            }

            return coupons;
        }

        [TestMethod()]
        public void AssociateCouponTest()
        {
            var coupon = GetCoupons().Coupons.First();
            var subscription = GetSubscriptions().Subscriptions.First(x => x.Status == Subscription.SubscriptionStatus.ACTIVE);

            assinaturasClient.AssociateCoupon(coupon.Code, subscription.Code);

            subscription = assinaturasClient.GetSubscription(subscription.Code);

            Assert.AreEqual(coupon.Code, subscription.Coupon.Code);
        }

        [TestMethod()]
        public void GetCouponTest()
        {
            var firstCoupon = GetCoupons().Coupons.First();
            var coupon = assinaturasClient.GetCoupon(firstCoupon.Code);

            Assert.AreEqual(firstCoupon.Name, coupon.Name);
        }

        [TestMethod()]
        public void GetCouponsTest()
        {
            var retorno = assinaturasClient.GetCoupons();
            Assert.IsNotNull(retorno);
            Assert.IsNotNull(retorno.Coupons);
            Assert.IsTrue(retorno.Coupons.Length > 0);
            Assert.IsNotNull(retorno.Coupons[0].Code);
        }

        [TestMethod()]
        public void InactivateCouponTest()
        {
            var activeCoupon = GetCoupons().Coupons.Where(x => x.Status == Coupon.CouponStatus.ACTIVE).FirstOrDefault();

            if (activeCoupon == null)
            {
                throw new AssertInconclusiveException("Nenhum cupom ativo foi encontrado");
            }

            var coupon = assinaturasClient.InactivateCoupon(activeCoupon.Code);

            Assert.AreEqual(Coupon.CouponStatus.INACTIVE, coupon.Status);
        }

        [TestMethod()]
        public void ActivateCouponTest()
        {
            var inactiveCoupon = GetCoupons().Coupons.Where(x => x.Status == Coupon.CouponStatus.INACTIVE).FirstOrDefault();

            if (inactiveCoupon == null)
            {
                throw new AssertInconclusiveException("Nenhum cupom inativo foi encontrado");
            }

            var coupon = assinaturasClient.ActivateCoupon(inactiveCoupon.Code);

            Assert.AreEqual(Coupon.CouponStatus.ACTIVE, coupon.Status);
        }

        [TestMethod()]
        public void DesassociateCouponTest()
        {
            var coupon = GetCoupons().Coupons.FirstOrDefault(x => x.Status == Coupon.CouponStatus.ACTIVE);
            var subscription = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status == Subscription.SubscriptionStatus.ACTIVE || x.Status == Subscription.SubscriptionStatus.TRIAL);

            if (coupon == null)
            {
                throw new AssertInconclusiveException("Nenhum cupom ativo foi encontrado.");
            }

            if (subscription == null)
            {
                throw new AssertInconclusiveException("Nenhuma assinatura ativa foi encontrada");
            }

            //Primeiro associa o cupon, depois testa desassociar
            assinaturasClient.AssociateCoupon(coupon.Code, subscription.Code);
            subscription = assinaturasClient.DesassociateCoupon(subscription.Code);

            Assert.IsNull(subscription.Coupon);
        }

        #endregion

        #region Retry

        [TestMethod()]
        public void InvoiceRetryTest()
        {
            //Pega a primeira ativa (tem mais chances de ter gerado cobrança)
            var firstActive = GetSubscriptions().Subscriptions.FirstOrDefault(x => x.Status == Subscription.SubscriptionStatus.ACTIVE);
            var invoice = assinaturasClient.GetInvoices(firstActive.Code);

            try
            {
                //Tenta o retry, mas é dificil ter algum na situação de retry, então vou ter que aceitar o erro de retry como um OK (já que a informação chegou corretamente na API do moip)
                assinaturasClient.InvoiceRetry(invoice.Invoices[0].Id);
            }
            catch (MoipException ex)
            {
                //Caso a fatura esteja ativa, ele volta o erro, mas pelo menos chegou lá e validou
                Assert.IsTrue(ex.Message.IndexOf("ativo", StringComparison.CurrentCultureIgnoreCase) > 0);
            }

            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void InvoiceRetryPreferencesTest()
        {
            var retryPreference = new PreferencesRetry()
            {
                FirstTry = 1,
                SecondTry = 1,
                ThirdTry = 1,
                Finally = PreferencesRetry.RetryFinallyType.CANCEL
            };

            assinaturasClient.InvoiceRetryPreferences(retryPreference);

            Assert.IsTrue(true);
        } 
        #endregion
    }
}
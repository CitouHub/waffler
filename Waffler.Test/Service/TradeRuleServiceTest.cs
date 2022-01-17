using System;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

using Waffler.Common;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Test.Helper;
using Waffler.Service.CustomException;

namespace Waffler.Test.Service
{
    public class TradeRuleServiceTest
    {
        private readonly ILogger<TradeRuleService> _logger = Substitute.For<ILogger<TradeRuleService>>();
        private readonly IMapper _mapper;

        public TradeRuleServiceTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public async Task NewTradeRule()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);

            //Act
            var tradeRule = await tradeRuleService.NewTradeRuleAsync();

            //Assert
            Assert.NotNull(tradeRule);
            Assert.NotNull(context.TradeRules.FirstOrDefault(_ => _.Id == tradeRule.Id));
        }

        [Fact]
        public async Task AddTradeRule_InvalidVolumePriceReferenceException()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.CandleStickValueTypeId = (short)Variable.CandleStickValueType.Volume;

            //Act & Assert
            _ = await Assert.ThrowsAsync<InvalidVolumePriceReferenceException>(() => tradeRuleService.AddTradeRuleAsync(tradeRule));
        }

        [Fact]
        public async Task AddTradeRule_NoConditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();

            //Act
            var success = await tradeRuleService.AddTradeRuleAsync(tradeRule);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeRules.FirstOrDefault(_ => _.Name == $"{tradeRule.Name} (Imported)"));
        }

        [Fact]
        public async Task AddTradeRule_Conditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);

            //Act
            var success = await tradeRuleService.AddTradeRuleAsync(tradeRule);

            //Assert
            var importedTradeRule = context.TradeRules.FirstOrDefault(_ => _.Name == $"{tradeRule.Name} (Imported)");
            Assert.True(success);
            Assert.NotNull(importedTradeRule);
            Assert.NotNull(context.TradeRuleConditions.FirstOrDefault(_ => _.TradeRuleId == importedTradeRule.Id && _.Description == $"{tradeRuleCondition.Description}"));
        }

        [Fact]
        public async Task CopyTradeRule_TradeRuleDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);

            //Act
            var success = await tradeRuleService.CopyTradeRuleAsync(-1);

            //Assert
            Assert.False(success);
            Assert.Empty(context.TradeRules.ToList());
            Assert.Empty(context.TradeRuleConditions.ToList());
        }

        [Fact]
        public async Task CopyTradeRule_DeletedTradeRule()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.IsDeleted = true;
            context.TradeRules.Add(tradeRule);
            context.SaveChanges();
            tradeRule = context.TradeRules.FirstOrDefault();

            //Act
            var success = await tradeRuleService.CopyTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.False(success);
            Assert.Single(context.TradeRules.ToList());
        }

        [Fact]
        public async Task CopyTradeRule_NoConditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();

            //Act
            var success = await tradeRuleService.CopyTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeRules.FirstOrDefault(_ => _.Name == $"{tradeRule.Name} (Copy)"));
        }

        [Fact]
        public async Task CopyTradeRule_Conditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition.TradeRuleId = tradeRule.Id;
            context.TradeRuleConditions.Add(tradeRuleCondition);
            context.SaveChanges();

            //Act
            var success = await tradeRuleService.CopyTradeRuleAsync(tradeRule.Id);

            //Assert
            var copiedTradeRule = context.TradeRules.FirstOrDefault(_ => _.Name == $"{tradeRule.Name} (Copy)");
            Assert.True(success);
            Assert.NotNull(copiedTradeRule);
            Assert.NotNull(context.TradeRuleConditions.FirstOrDefault(_ => _.TradeRuleId == copiedTradeRule.Id && _.Description == $"{tradeRuleCondition.Description}"));
        }

        [Fact]
        public async Task GetTradeRules_Empty()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            
            //Act
            var tradeRules = await tradeRuleService.GetTradeRulesAsync();

            //Assert
            Assert.Empty(tradeRules);
        }

        [Fact]
        public async Task GetTradeRules_TradeRuleIsDeleted()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.IsDeleted = true;
            context.TradeRules.Add(tradeRule);
            context.SaveChanges();

            //Act
            var tradeRules = await tradeRuleService.GetTradeRulesAsync();

            //Assert
            Assert.Empty(tradeRules);
        }

        [Fact]
        public async Task GetTradeRules()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition.TradeRuleId = tradeRule.Id;
            context.TradeRuleConditions.Add(tradeRuleCondition);
            context.SaveChanges();

            //Act
            var tradeRules = await tradeRuleService.GetTradeRulesAsync();

            //Assert
            Assert.Equal(context.TradeRules.Count(), tradeRules.Count);
            Assert.Equal(context.TradeRuleConditions.Count(), tradeRules.Sum(_ => _.TradeRuleConditions.Count));
        }

        [Fact]
        public async Task GetTradeRuleConditionAttributes()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);

            //Act
            var attributes = await tradeRuleService.GetTradeRuleAttributesAsync();

            //Assert
            Assert.NotNull(attributes);
        }

        [Fact]
        public async Task GetTradeRule_TradeRuleDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);

            //Act
            var tradeRule = await tradeRuleService.GetTradeRuleAsync(-1);

            //Assert
            Assert.Null(tradeRule);
        }

        [Fact]
        public async Task GetTradeRule_TradeRuleDoesIsDeleted()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.IsDeleted = true;
            context.TradeRules.Add(tradeRule);
            context.SaveChanges();
            tradeRule = context.TradeRules.FirstOrDefault();

            //Act
            var tradeRuleDto = await tradeRuleService.GetTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.Null(tradeRuleDto);
        }

        [Fact]
        public async Task GetTradeRule_TradeRuleExists_NoConditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();

            //Act
            var foundTradeRule = await tradeRuleService.GetTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.NotNull(foundTradeRule);
            Assert.Empty(foundTradeRule.TradeRuleConditions);
        }

        [Fact]
        public async Task GetTradeRule_TradeRuleExists_Conditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();
            var tradeRuleCondition1 = TradeRuleConditionHelper.GetTradeRuleCondition();
            var tradeRuleCondition2 = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition1.TradeRuleId = tradeRule.Id;
            tradeRuleCondition2.TradeRuleId = tradeRule.Id;
            context.TradeRuleConditions.Add(tradeRuleCondition1);
            context.TradeRuleConditions.Add(tradeRuleCondition2);
            context.SaveChanges();

            //Act
            var foundTradeRule = await tradeRuleService.GetTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.NotNull(foundTradeRule);
            Assert.Equal(context.TradeRuleConditions.Count(), foundTradeRule.TradeRuleConditions.Count);
        }

        [Fact]
        public async Task SetupTradeRuleTest_TradeRuleDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);

            //Act
            var success = await tradeRuleService.SetupTradeRuleTestAsync(-1);

            //Assert
            Assert.False(success);
        }

        [Fact]
        public async Task SetupTradeRuleTest_TradeRuleExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();

            //Act
            var success = await tradeRuleService.SetupTradeRuleTestAsync(tradeRule.Id);

            //Assert
            tradeRule = context.TradeRules.FirstOrDefault();
            Assert.True(success);
            Assert.Equal(DateTime.MinValue, tradeRule.LastTrigger);
            Assert.Equal((short)Variable.TradeRuleStatus.Test, tradeRule.TradeRuleStatusId);
        }

        [Fact]
        public async Task UpdateTradeRule_InvalidVolumePriceReferenceException()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.CandleStickValueTypeId = (short)Variable.CandleStickValueType.Volume;

            //Act & Assert
            _ = await Assert.ThrowsAsync<InvalidVolumePriceReferenceException>(() => tradeRuleService.UpdateTradeRuleAsync(tradeRule));
        }

        [Fact]
        public async Task UpdateTradeRule_TradeRuleDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Id = -1;

            //Act
            var success = await tradeRuleService.UpdateTradeRuleAsync(tradeRule);

            //Assert
            Assert.False(success);
        }

        [Fact]
        public async Task UpdateTradeRule_TradeRuleExists()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();
            var tradeRuleDTO = TradeRuleHelper.GetTradeRuleDTO();
            tradeRuleDTO.Id = tradeRule.Id;
            tradeRuleDTO.Name = Guid.NewGuid().ToString();

            //Act
            var success = await tradeRuleService.UpdateTradeRuleAsync(tradeRuleDTO);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeRules.FirstOrDefault(_ => _.Name == tradeRuleDTO.Name));
        }

        [Fact]
        public async Task DeleteTradeRule_TradeRuleDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);

            //Act
            var success = await tradeRuleService.DeleteTradeRuleAsync(-1);

            //Assert
            Assert.False(success);
        }

        [Fact]
        public async Task DeleteTradeRule_TradeRuleExists_NoConditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();

            //Act
            var success = await tradeRuleService.DeleteTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeRules.FirstOrDefault(_ => _.Id == tradeRule.Id && _.IsDeleted));
        }

        [Fact]
        public async Task DeleteTradeRule_TradeRuleExists_Conditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition.TradeRuleId = tradeRule.Id;
            context.TradeRuleConditions.Add(tradeRuleCondition);
            context.SaveChanges();

            //Act
            var success = await tradeRuleService.DeleteTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeRules.FirstOrDefault(_ => _.Id == tradeRule.Id && _.IsDeleted));
        }

        [Fact]
        public async Task DeleteTradeRule_TradeOrders()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleService = new TradeRuleService(_logger, context, _mapper);
            context.TradeRules.Add(TradeRuleHelper.GetTradeRule());
            context.SaveChanges();
            var tradeRule = context.TradeRules.FirstOrDefault();

            var liveOrder = TradeOrderHelper.GetTradeOrder();
            liveOrder.TradeRuleId = tradeRule.Id;
            liveOrder.TradeOrderStatusId = (short)Variable.TradeOrderStatus.Open;
            var testOrder = TradeOrderHelper.GetTradeOrder();
            testOrder.TradeRuleId = tradeRule.Id;
            testOrder.TradeOrderStatusId = (short)Variable.TradeOrderStatus.Test;
            context.TradeOrders.Add(liveOrder);
            context.TradeOrders.Add(testOrder);
            context.SaveChanges();

            //Act
            var success = await tradeRuleService.DeleteTradeRuleAsync(tradeRule.Id);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeRules.FirstOrDefault(_ => _.Id == tradeRule.Id && _.IsDeleted));
            Assert.NotNull(context.TradeOrders.FirstOrDefault(_ => _.Id == liveOrder.Id));
            Assert.Null(context.TradeRules.FirstOrDefault(_ => _.Id == testOrder.Id));
        }
    }
}

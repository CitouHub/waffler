using System;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

using Waffler.Data;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Test.Helper;

namespace Waffler.Test.Service
{
    public class TradeRuleConditionServiceTest
    {
        private readonly ILogger<TradeRuleConditionService> _logger = Substitute.For<ILogger<TradeRuleConditionService>>();
        private readonly IMapper _mapper;

        public TradeRuleConditionServiceTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        private static TradeRule AddTradeRule(WafflerDbContext context)
        {
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.Id = context.TradeRules.Count() + 1;
            context.TradeRules.Add(tradeRule);
            context.SaveChanges();
            return context.TradeRules.FirstOrDefault(_ => _.Id == tradeRule.Id);
        }

        [Fact]
        public async Task NewTradeRuleCondition_TradeRuleDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);

            //Act
            var tradeRuleCondition = await tradeRuleConditionService.NewTradeRuleConditionAsync(-1);

            //Assert
            Assert.Null(tradeRuleCondition);
        }

        [Fact]
        public async Task NewTradeRuleCondition_TradeRuleExists()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRule = AddTradeRule(context);
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);

            //Act
            var tradeRuleCondition = await tradeRuleConditionService.NewTradeRuleConditionAsync(tradeRule.Id);

            //Assert
            Assert.NotNull(tradeRuleCondition);
        }

        [Fact]
        public async Task GetTradeRuleConditions()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRule1 = AddTradeRule(context);
            var tradeRule2 = AddTradeRule(context);
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);
            var tradeRuleCondition1 = TradeRuleConditionHelper.GetTradeRuleCondition();
            var tradeRuleCondition2 = TradeRuleConditionHelper.GetTradeRuleCondition();
            var tradeRuleCondition3 = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition1.TradeRuleId = tradeRule1.Id;
            tradeRuleCondition2.TradeRuleId = tradeRule1.Id;
            tradeRuleCondition3.TradeRuleId = tradeRule2.Id;
            context.TradeRuleConditions.Add(tradeRuleCondition1);
            context.TradeRuleConditions.Add(tradeRuleCondition2);
            context.TradeRuleConditions.Add(tradeRuleCondition3);
            context.SaveChanges();

            //Act
            var tradeRuleConditions = await tradeRuleConditionService.GetTradeRuleConditionsAsync(tradeRule1.Id);

            //Assert
            Assert.True(tradeRuleConditions.All(_ => _.TradeRuleId == tradeRule1.Id));
            Assert.Equal(context.TradeRuleConditions.Count(_ => _.TradeRuleId == tradeRule1.Id), tradeRuleConditions.Count);
        }

        [Fact]
        public async Task GetTradeRuleConditionAttributes()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);

            //Act
            var attributes = await tradeRuleConditionService.GetTradeRuleConditionAttributesAsync();

            //Assert
            Assert.NotNull(attributes);
        }

        [Fact]
        public async Task UpdateTradeRuleCondition_TradeRuleConditionDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            tradeRuleCondition.Id = -1;

            //Act
            var success = await tradeRuleConditionService.UpdateTradeRuleConditionAsync(tradeRuleCondition);

            //Assert
            Assert.False(success);
        }

        [Fact]
        public async Task UpdateTradeRuleCondition_TradeRuleConditionExists()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);
            var tradeRule = AddTradeRule(context);
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition.TradeRuleId = tradeRule.Id;
            context.TradeRuleConditions.Add(tradeRuleCondition);
            context.SaveChanges();
            var tradeRuleConditionDTO = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            tradeRuleConditionDTO.Id = tradeRuleCondition.Id;
            tradeRuleConditionDTO.TradeRuleId = tradeRule.Id;
            tradeRuleConditionDTO.Description = Guid.NewGuid().ToString();
            

            //Act
            var success = await tradeRuleConditionService.UpdateTradeRuleConditionAsync(tradeRuleConditionDTO);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeRuleConditions.FirstOrDefault(_ => _.Description == tradeRuleConditionDTO.Description));
        }

        [Fact]
        public async Task DeleteTradeRuleCondition_TradeRuleConditionDoesNotExist()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);

            //Act
            var success = await tradeRuleConditionService.DeleteTradeRuleConditionAsync(-1);

            //Assert
            Assert.False(success);
        }

        [Fact]
        public async Task DeleteTradeRuleCondition_TradeRuleConditionExists()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeRuleConditionService = new TradeRuleConditionService(_logger, context, _mapper);
            var tradeRule = AddTradeRule(context);
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition.TradeRuleId = tradeRule.Id;
            context.TradeRuleConditions.Add(tradeRuleCondition);
            context.SaveChanges();

            //Act
            var success = await tradeRuleConditionService.DeleteTradeRuleConditionAsync(tradeRuleCondition.Id);

            //Assert
            Assert.True(success);
            Assert.Null(context.TradeRuleConditions.FirstOrDefault(_ => _.Id == tradeRule.Id));
        }
    }
}

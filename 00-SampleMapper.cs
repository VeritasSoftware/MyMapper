using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMapper.Test.Mappers
{
    using MyMapper;    
    using MyMapper.Converters;
    using MyMapper.Test.Entities;

    public interface IResponseMapper
    {
        Response3 Map(Response1 response1);
    }

    public class ResponseMapper : IResponseMapper
    {
        public ResponseMapper()
        {
        }

        public Fund3 Map(Fund1 fund1)
        {
            //Both Classes (Fund1 & Fund3) have properties by the same name
            return Mapper<Fund1, Fund3>.Exec<EntityConverter<Fund1, Fund3>>(fund1);
        }

        public Details3 Map(Details1 details1)
        {
            return Mapper<Details1, Details3>.Map(details1)
                                               .With(d1 => d1.DOB, (d3, dob) => d3.DateOfBirth = dob)
                                               .With(d1 => d1.IsDisabled, (d3, disabled) => d3.IsHandicapped = disabled)
                                             .Exec();
        }

        public BankingInfo3 Map(BankingInfo1 bankingInfo1)
        {
            return Mapper<BankingInfo1, BankingInfo3>.Map(bankingInfo1)
                                                        .With(bi1 => bi1.AccountNo, (bi3, accNo) => bi3.AccountNumber = accNo)
                                                     .Exec();
        }

        public Response3 Map(Response1 response1)
        {
            return Mapper<Response1, Response3>.Map(response1)
                                                    .With(r1 => r1.ConsumerID, (r3, consumerId) => r3.IDNumber = consumerId)
                                                    //Calculated field
                                                    .With(r1 => r1.AvgNoOfPurchasesPerMonth * r1.PeriodInMonths, (r3, total) => r3.TotalPurchases = total)
                                                    //Mapping List
                                                    .With(r1 => r1.BankingInfos, (r3, bankingInfos) => r3.BankingInformation = bankingInfos, Map)
                                                    //Using another map - When Details1 is not null then map Details1 to Details3 using another map 
                                                    .When(r1 => r1.Details != null, mapper => mapper.With(r1 => r1.Details, (r3, details3) => r3.Details = details3, Map))
                                                    //Using another map - When Fund1 is not null then map Fund1 to Fund3 using another map 
                                                    .When(r1 => r1.Fund != null, mapper => mapper.With(r1 => r1.Fund, (r3, fund3) => r3.Fund = fund3, Map))
                                                .Exec();
        }
    }
}

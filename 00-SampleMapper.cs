using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics; //For Debugger.Break()

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
            //Both Classes (Fund1 & Fund3) have properties by the same name. Auto mapping (reflective) can be used.
            return Mapper<Fund1, Fund3>.Map(fund1).Exec();
        }

        public IDDocument Map(Passport passport)
        {
            return Mapper<Passport, IDDocument>.Map(passport, false) //Turn off auto mapping (reflective) with false. The properties with the same name will not be mapped then.
                                               .With(d1 => d1.PassportNo, (d3, passportNo) => d3.DocumentNumber = passportNo)
                                               .With(d1 => d1.DateOfIssue, (d3, dateOfIssue) => d3.IssueDate = dateOfIssue)
                                             .Exec();
        }

        public Details3 Map(Details1 details1)
        {
            return Mapper<Details1, Details3>.Map(details1, false)
                                               .With(d1 => d1.DOB, (d3, dob) => d3.DateOfBirth = dob)
                                               .With(d1 => d1.IsDisabled, (d3, disabled) => d3.IsHandicapped = disabled)
                                               //Mapping with another map. Using the Passport to IDDocument map.
                                               .With(d1 => d1.Passport, (d3, passport) => d3.ID = passport, Map)
                                             .Exec();
        }

        public BankingInfo3 Map(BankingInfo1 bankingInfo1)
        {
            return Mapper<BankingInfo1, BankingInfo3>.Map(bankingInfo1)
                                                        .With(bi1 => bi1.AccountNo, (bi3, accNo) => bi3.AccountNumber = accNo)
                                                     .Exec();
        }

        public InsuranceInfo Map(InsuranceMutualFund insuranceMutualFund)
        {
            return Mapper<InsuranceMutualFund, InsuranceInfo>.Map(insuranceMutualFund)
                                                                .With(imf => imf.MutualFundNumber, (insuranceInfo, mutualFundNumber) => insuranceInfo.MembershipNo = mutualFundNumber)
                                                                .With(imf => imf.TaxNo, (insuranceInfo, taxNo) => insuranceInfo.TaxNumber= taxNo)
                                                             .Exec();
        }

        public InsuranceInfo Map(InsuranceSuperannuation insuranceSuperannuation)
        {
            return Mapper<InsuranceSuperannuation, InsuranceInfo>.Map(insuranceSuperannuation)
                                                                    .With(imf => imf.SuperannuationNumber, (insuranceInfo, mutualFundNumber) => insuranceInfo.MembershipNo = mutualFundNumber)
                                                                    .With(imf => imf.TaxFileNumber, (insuranceInfo, taxFileNo) => insuranceInfo.TaxNumber = taxFileNo)
                                                                 .Exec();
        }

        public InsuranceInfo Map(InsuranceEmployment insuranceEmployment)
        {
            return Mapper<InsuranceEmployment, InsuranceInfo>.Map(insuranceEmployment)
                                                                    .With(ie => ie.EmploymentNumber, (insuranceInfo, employmentNumber) => insuranceInfo.MembershipNo = employmentNumber)
                                                                    .With(imf => imf.TaxNumber, (insuranceInfo, taxNumber) => insuranceInfo.TaxNumber = taxNumber)
                                                                 .Exec();
        }    

        public Response3 Map(Response1 response1)
        {            
            return Mapper<Response1, Response3>.Map(response1)
                                                    .With(r1 => r1.ConsumerID, (r3, consumerId) => r3.IDNumber = consumerId)
                                                    //Calculated field
                                                    .With(r1 => r1.AvgNoOfPurchasesPerMonth * r1.PeriodInMonths, (r3, total) => r3.TotalPurchases = total)
                                                    //Using Switch
                                                    .Switch(r1 => r1.PeriodInMonths)                
                                                        .Case(periodInMonths => periodInMonths > 0 && periodInMonths <= 3, (r3, periodInMonths) => r3.Period = "Quarter")
                                                        .Case(periodInMonths => periodInMonths > 3 && periodInMonths <= 6, (r3, periodInMonths) => r3.Period = "Half")
                                                        .Case(periodInMonths => periodInMonths > 6 && periodInMonths <= 9, (r3, periodInMonths) => r3.Period = "Three Quarter")
                                                        .Case(periodInMonths => periodInMonths > 9 && periodInMonths <= 12, (r3, periodInMonths) => r3.Period = "Year")
                                                        .Else((r3, periodInMonths) => r3.Period = "Unknown")
                                                    .End()
                                                    //Switch mapping with case mappings
                                                    .Switch(r1 => r1.InsuranceType)
                                                        .CaseMap(insuranceType => insuranceType == InsuranceType.MutualFund,
                                                                        //Mapping source InsuranceMutualFund to destination InsuranceInfo using Map
                                                                        //You can debug using using Debugger.Break()
                                                                        mapper => { Debugger.Break(); mapper.With(r1 => r1.InsuranceMutualFund, (r3, insuranceMutualFund) => r3.InsuranceInfo = insuranceMutualFund, Map); }
                                                                 )
                                                        .CaseMap(insuranceType => insuranceType == InsuranceType.Superannuation,
                                                                        //Mapping source InsuranceSuperannuation to destination InsuranceInfo using Map
                                                                        mapper => mapper.With(r1 => r1.InsuranceSuperannuation, (r3, insuranceSuperannuation) => r3.InsuranceInfo = insuranceSuperannuation, Map)
                                                                 )
                                                        //Mapping source InsuranceEmployment to destination InsuranceInfo using Map by default
                                                        .ElseMap(mapper => mapper.With(r1 => r1.InsuranceEmployment, (r3, insuranceEmployment) => r3.InsuranceInfo = insuranceEmployment, Map))
                                                    .End()
                                                    //Mapping List
                                                    .With(r1 => r1.BankingInfos, (r3, bankingInfos) => r3.BankingInformation = bankingInfos, Map)
                                                    //Conditional mapping with another map - When Details1 is not null then map Details1 to Details3 using another map 
                                                    .When(r1 => r1.Details != null, mapper => mapper.With(r1 => r1.Details, (r3, details3) => r3.Details = details3, Map))
                                                    //Conditional mapping with another map - When Fund1 is not null then map Fund1 to Fund3 using another map 
                                                    .When(r1 => r1.MutualFund != null, mapper => mapper.With(r1 => r1.MutualFund, (r3, fund3) => r3.Fund = fund3, Map))
                                                .Exec();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMapper.Test.Entities
{
    #region Response1
    public class Details1
    {
        public DateTime DOB { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class Address1
    {
        public string StreetNo { get; set; }

        public State1 State { get; set; }

        public Country1 Country { get; set; }
    }

    public class State1
    {
        public string Name { get; set; }
        public string Abbr { get; set; }
    }

    public class Country1
    {
        public string Name { get; set; }
        public string Abbr { get; set; }
    }

    public class Fund1
    {
        public string Name { get; set; }
        public int FundId { get; set; }

        public string[] OtherNames { get; set; }

        public Dictionary<int, int> FundKeys { get; set; }

        public Address1 Address { get; set; }
    }

    public class BankingInfo1
    {
        public string AccountNo { get; set; }
        //Automapped
        public string AccountName { get; set; }
    }    

    public class InsuranceMutualFund
    {
        public string MutualFundNumber { get; set; }
        public string TaxNo { get; set; }
    }

    public class InsuranceSuperannuation
    {
        public string SuperannuationNumber { get; set; }
        public string TaxFileNumber { get; set; }
    }

    public class InsuranceEmployment
    {
        public string EmploymentNumber { get; set; }
        public string TaxNumber { get; set; }
    }

    public enum InsuranceType
    {
        MutualFund = 0,
        Superannuation = 1,
        Employment
    }

    public class Response1
    {
        public int ConsumerID { get; set; }
        public string Name { get; set; }

        public int AvgNoOfPurchasesPerMonth { get; set; }
        public int PeriodInMonths { get; set; }

        public Details1 Details { get; set; }

        public Fund1 MutualFund { get; set; }         

        //List
        public List<BankingInfo1> BankingInfos { get; set; }

        //bool HasInsuranceInfo { get; set; }

        public InsuranceMutualFund InsuranceMutualFund { get; set; }

        public InsuranceSuperannuation InsuranceSuperannuation { get; set; }

        public InsuranceEmployment InsuranceEmployment { get; set; }

        public InsuranceType InsuranceType { get; set; }
    }
    #endregion

    #region Response3
    public class Details3
    {
        public DateTime DateOfBirth { get; set; }

        public bool IsHandicapped { get; set; }
    }

    public class Address3
    {
        public string StreetNo { get; set; }

        public State3 State { get; set; }

        public Country3 Country { get; set; }
    }

    public class State3
    {
        public string Name { get; set; }
        public string Abbr { get; set; }
    }

    public class Country3
    {
        public string Name { get; set; }
        public string Abbr { get; set; }
    }

    public class Fund3
    {
        public string Name { get; set; }
        public int FundId { get; set; }

        public string[] OtherNames { get; set; }

        public Dictionary<short, short> FundKeys { get; set; }

        public Address3 Address { get; set; }
    }

    public class BankingInfo3
    {
        public string AccountNumber { get; set; }
        //Automapped
        public string AccountName { get; set; }
    }    

    public class InsuranceInfo
    {
        public string MembershipNo { get; set; }
        public string TaxNumber { get; set; }
    }

    public class Response3
    {
        public int IDNumber { get; set; }
        public string Name { get; set; }

        //Calculated Field
        public int TotalPurchases { get; set; }

        public Details3 Details { get; set; }

        //Automapped
        public Fund3 Fund { get; set; }

        //List
        public List<BankingInfo3> BankingInformation { get; set; }

        public string Period { get; set; }

        public InsuranceInfo InsuranceInfo { get; set; }
    }   
    #endregion
}

using MyMapper.Test.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyMapper.Test.Mappers
{
    public interface IMapper
    {
        Details3 Map(Details1 details1);

        Task<Details3> MapAsync(Details1 details1);

        IEnumerable<Details3> Map(IEnumerable<Details1> detailsList);

        Task<IEnumerable<Details3>> MapAsync(IEnumerable<Details1> detailsList);

        Fund3 Map(Fund1 fund1);

        Task<Fund3> MapAsync(Fund1 fund1);

        IEnumerable<Fund3> Map(IEnumerable<Fund1> fundsList);

        Task<IEnumerable<Fund3>> MapAsync(IEnumerable<Fund1> fundsList);
    }

    public class Mapper : IMapper
    {
        //Mapping Rules for mapping from Details1 to Details3
        private Details3 MapRules(IMyMapperRules<Details1, Details3> myMapper)
        {
            return myMapper.With(s => s.DOB, (d, dob) => d.DateOfBirth = dob)
                           .With(s => s.IsDisabled, (d, disabled) => d.IsHandicapped = disabled)
                           .Exec();
        }

        //Specify MyMapper mapping rules - turn off automapping
        //Details1 and Details3 have different property names - specify mapping rules
        public Details3 Map(Details1 details1)
        {
            return details1.Map<Details1, Details3>(MapRules, false);
        }

        //Same as above - using Async
        public async Task<Details3> MapAsync(Details1 details1)
        {
            return await details1.MapAsync<Details1, Details3>(MapRules, false);
        }

        //Specify MyMapper mapping rules - turn off automapping
        //Details1 and Details3 have different property names - specify mapping rules
        public IEnumerable<Details3> Map(IEnumerable<Details1> detailsList)
        {
            return detailsList.Map<Details1, Details3>(MapRules, false);
        }

        //Same as above - using Async
        public async Task<IEnumerable<Details3>> MapAsync(IEnumerable<Details1> detailsList)
        {
            return await detailsList.MapAsync<Details1, Details3>(MapRules, false);
        }

        //Using automapping (for classes with the same property names).
        //Fund1 and Fund3 (and their contained classes) have the same property names
        public Fund3 Map(Fund1 fund1)
        {
            return fund1.Map<Fund1, Fund3>();
        }

        //Same as above - using Async
        public async Task<Fund3> MapAsync(Fund1 fund1)
        {
            return await fund1.MapAsync<Fund1, Fund3>();
        }

        //Using automapping (for classes with the same property names).
        //Fund1 and Fund3 (and their contained classes) have the same property names
        public IEnumerable<Fund3> Map(IEnumerable<Fund1> fundsList)
        {
            return fundsList.Map<Fund1, Fund3>();
        }

        //Same as above - using Async
        public async Task<IEnumerable<Fund3>> MapAsync(IEnumerable<Fund1> fundsList)
        {
            return await fundsList.MapAsync<Fund1, Fund3>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using EFCoreRelationshipsPractice;
using EFCoreRelationshipsPractice.Dtos;
using EFCoreRelationshipsPractice.Repository;
using EFCoreRelationshipsPractice.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace EFCoreRelationshipsPracticeTest
{
    public class CompanyControllerTest : TestBase
    {
        public CompanyControllerTest(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Should_create_company_employee_profile_success()
        {
            var client = GetClient();
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19
                }
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var httpContent = JsonConvert.SerializeObject(companyDto);
            StringContent content = new StringContent(httpContent, Encoding.UTF8, MediaTypeNames.Application.Json);
            await client.PostAsync("/companies", content);

            var allCompaniesResponse = await client.GetAsync("/companies");
            var body = await allCompaniesResponse.Content.ReadAsStringAsync();

            var returnCompanies = JsonConvert.DeserializeObject<List<CompanyDto>>(body);

            Assert.Equal(1, returnCompanies.Count);
            Assert.Equal(companyDto.Employees.Count, returnCompanies[0].Employees.Count);
            Assert.Equal(companyDto.Employees[0].Age, returnCompanies[0].Employees[0].Age);
            Assert.Equal(companyDto.Employees[0].Name, returnCompanies[0].Employees[0].Name);
            Assert.Equal(companyDto.Profile.CertId, returnCompanies[0].Profile.CertId);
            Assert.Equal(companyDto.Profile.RegisteredCapital, returnCompanies[0].Profile.RegisteredCapital);

            var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<CompanyDbContext>();
            Assert.Equal(1, context.Companies.ToList().Count);
            var firstCompany = await context.Companies.Include(c => c.Profile).FirstOrDefaultAsync();
            Assert.Equal(companyDto.Profile.CertId, firstCompany.Profile.CertId);
        }

        [Fact]
        public async Task Should_delete_company_and_related_employee_and_profile_success()
        {
            var client = GetClient();
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19
                }
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var httpContent = JsonConvert.SerializeObject(companyDto);
            StringContent content = new StringContent(httpContent, Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await client.PostAsync("/companies", content);
            await client.DeleteAsync(response.Headers.Location);
            var allCompaniesResponse = await client.GetAsync("/companies");
            var body = await allCompaniesResponse.Content.ReadAsStringAsync();

            var returnCompanies = JsonConvert.DeserializeObject<List<CompanyDto>>(body);

            Assert.Equal(0, returnCompanies.Count);
            Assert.Equal(0, returnCompanies.Count);
            Assert.Equal(0, returnCompanies.Count);
        }

        [Fact]
        public async Task Should_create_many_companies_success()
        {
            var client = GetClient();
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19
                }
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var httpContent = JsonConvert.SerializeObject(companyDto);
            StringContent content = new StringContent(httpContent, Encoding.UTF8, MediaTypeNames.Application.Json);
            await client.PostAsync("/companies", content);
            await client.PostAsync("/companies", content);

            var allCompaniesResponse = await client.GetAsync("/companies");
            var body = await allCompaniesResponse.Content.ReadAsStringAsync();

            var returnCompanies = JsonConvert.DeserializeObject<List<CompanyDto>>(body);

            Assert.Equal(2, returnCompanies.Count);
        }

        [Fact]
        public async Task Should_get_all_company_via_service()
        {
            var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            CompanyDbContext context = scopedServices.GetRequiredService<CompanyDbContext>();
            context.Companies.RemoveRange(context.Companies);
            context.SaveChanges();

            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
                {
                    new EmployeeDto()
                    {
                        Name = "Tom",
                        Age = 19,
                    },
                };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            CompanyService companyService = new CompanyService(context);
            await companyService.AddCompany(companyDto);
            var returnCompanies = await companyService.GetAll();

            Assert.Equal(1, returnCompanies.Count());
            Assert.Equal(1, returnCompanies.Count);
            Assert.Equal(companyDto.Employees.Count, returnCompanies[0].Employees.Count);
            Assert.Equal(companyDto.Employees[0].Age, returnCompanies[0].Employees[0].Age);
            Assert.Equal(companyDto.Employees[0].Name, returnCompanies[0].Employees[0].Name);
            Assert.Equal(companyDto.Profile.CertId, returnCompanies[0].Profile.CertId);
            Assert.Equal(companyDto.Profile.RegisteredCapital, returnCompanies[0].Profile.RegisteredCapital);
        }

        [Fact]
        public async Task Should_get_company_by_id_via_service()
        {
            var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            CompanyDbContext context = scopedServices.GetRequiredService<CompanyDbContext>();
            context.Companies.RemoveRange(context.Companies);
            context.SaveChanges();

            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
                {
                    new EmployeeDto()
                    {
                        Name = "Tom",
                        Age = 19,
                    },
                };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            CompanyService companyService = new CompanyService(context);
            var id = await companyService.AddCompany(companyDto);
            var returnCompany = await companyService.GetById(id);

            Assert.Equal(companyDto.Employees.Count, returnCompany.Employees.Count);
            Assert.Equal(companyDto.Employees[0].Age, returnCompany.Employees[0].Age);
            Assert.Equal(companyDto.Employees[0].Name, returnCompany.Employees[0].Name);
            Assert.Equal(companyDto.Profile.CertId, returnCompany.Profile.CertId);
            Assert.Equal(companyDto.Profile.RegisteredCapital, returnCompany.Profile.RegisteredCapital);
        }

        [Fact]
        public async Task Should_create_company_via_service()
        {
            var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            CompanyDbContext context = scopedServices.GetRequiredService<CompanyDbContext>();
            context.Companies.RemoveRange(context.Companies);
            context.SaveChanges();

            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
                {
                    new EmployeeDto()
                    {
                        Name = "Tom",
                        Age = 19,
                    },
                };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };
            Assert.Equal(0, context.Companies.Count());
            CompanyService companyService = new CompanyService(context);
            await companyService.AddCompany(companyDto);
            Assert.Equal(1, context.Companies.Count());
        }

        [Fact]
        public async Task Should_delete_company_via_service()
        {
            var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            CompanyDbContext context = scopedServices.GetRequiredService<CompanyDbContext>();
            context.Companies.RemoveRange(context.Companies);
            context.SaveChanges();

            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
                {
                    new EmployeeDto()
                    {
                        Name = "Tom",
                        Age = 19,
                    },
                };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            CompanyService companyService = new CompanyService(context);
            var id = await companyService.AddCompany(companyDto);

            await companyService.DeleteCompany(id);

            Assert.Equal(0, context.Companies.Count());
        }
    }
}
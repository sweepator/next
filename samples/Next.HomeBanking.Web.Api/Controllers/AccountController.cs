using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Next.HomeBanking.Application.Commands;
using Next.HomeBanking.Application.Queries;
using Next.Web.Application.Controllers;
using Next.Web.Application.PortAdapters;
using Next.Web.Binders;

namespace Next.HomeBanking.Web.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/accounts")]
    public class AccountController: BaseController
    {
        public AccountController(IHttpPortAdapter httpPortAdapter) 
            : base(httpPortAdapter)
        {
        }

        [HttpPost(Name = RouteNames.CreateAccount)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
        {
            return await PortAdapter.Execute(command);
        }
        
        [HttpPost("{id}/deposit", Name = RouteNames.Deposit)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Deposit([FromBodyAndRoute] DepositCommand command)
        {
            return await PortAdapter.Execute(command);
        }
        
        [HttpPost("{id}/debit", Name = RouteNames.Debit)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Debit([FromBodyAndRoute] DebitCommand command)
        {
            return await PortAdapter.Execute(command);
        }
        
        [HttpPost("{id}/transfer", Name = RouteNames.Transfer)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Transfer([FromBodyAndRoute] TransferCommand command)
        {
            return await PortAdapter.Execute(command);
        }
        
        [HttpPut("{id}/enable", Name = RouteNames.EnableAccount)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> EnableAccount([FromBodyAndRoute] EnableAccountCommand command)
        {
            return await PortAdapter.Execute(command);
        }

        [HttpGet(Name = RouteNames.GetAccounts)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAccounts([FromQuery] GetAccountsRequest query)
        {
            return await PortAdapter.Execute(query);
        }

        [HttpGet("{id}/transactions", Name = RouteNames.GetAccountTransactions)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetTransactions([FromRouteAndQuery] GetAccountTransactionsRequest query)
        {
            return await PortAdapter.Execute(query);
        }

        [HttpGet("{id}", Name = RouteNames.GetAccountDetails)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetDetails([FromRoute] GetAccountDetailsRequest query)
        {
            return await PortAdapter.ExecuteSingle(query);
        }
        
        [HttpDelete("{id}", Name = RouteNames.CancelAccount)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CancelAccount([FromRoute] CancelAccountCommand command)
        {
            return await PortAdapter.Execute(command);
        }

        [HttpDelete("{id}/transactions/{transactionId}", Name = RouteNames.CancelTransaction)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CancelTransaction([FromRoute] CancelTransactionCommand command)
        {
            return await PortAdapter.Execute(command);
        }
    }
}
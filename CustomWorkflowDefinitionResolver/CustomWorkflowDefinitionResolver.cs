using System;
using System.Collections.Generic;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Configuration;
using Telerik.Sitefinity.Web;
using Telerik.Sitefinity.Workflow;
using Telerik.Sitefinity.Workflow.Model;

namespace Telerik.Sitefinity.Samples.CustomWorkflowDefinitionResolver
{
    public class CustomWorkflowDefinitionResolver : WorkflowDefinitionResolver
    {
        public CustomWorkflowDefinitionResolver()
        {
            this.workflowForProducts = this.CreateWorkflowForProducts();
            this.workflowForResources = this.CreateWorkflowForResources();
            this.workflowForDocuments = base.GetWorkflowExecutionDefinitionByTitle("Workflow for Documents");
        }

        public override IWorkflowExecutionDefinition ResolveWorkflowExecutionDefinition(IWorkflowResolutionContext context)
        {
            // Return pre-created workflows for "Products", "Resources" and "Documents".
            if (this.IsPageUnder(context, "Products"))
                return this.workflowForProducts;
            if (this.IsPageUnder(context, "Resources"))
                return this.workflowForResources;
            if (this.IsPageUnder(context, "Documents") && this.workflowForDocuments != null)
                return this.workflowForDocuments;

            // The default constructor of WorkflowExecutionDefinitionProxy will instantiate
            // a simple no-approval-needed workflow.
            else if (this.IsPageUnder(context, "Forum"))
                return WorkflowDefinitionProxy.DefaultWorkflow;

            return base.ResolveWorkflowExecutionDefinition(context);
        }

        public override IWorkflowExecutionDefinition GetWorkflowExecutionDefinition(Guid id)
        {
            // Here we need to handle ids of the definitions that we created in memory.
            // There is no need to handle the id of 'workflowForDocuments', because
            // it is persisted in the database and the call to the base class will take
            // care of it.
            if (id == this.workflowForProducts.Id)
                return this.workflowForProducts;
            else if (id == this.workflowForResources.Id)
                return this.workflowForResources;
            else
                return base.GetWorkflowExecutionDefinition(id);
        }

        // Checks whether we are under a top-level page with a certain title.
        private bool IsPageUnder(IWorkflowResolutionContext context, string topLevelTitle)
        {
            if (typeof(PageNode).IsAssignableFrom(context.ContentType) &&
                context.ContentId != Guid.Empty)
            {
                var siteMap = SitefinitySiteMap.GetCurrentProvider() as SiteMapBase;
                PageSiteNode pageSiteNode = (PageSiteNode)siteMap.FindSiteMapNodeFromKey(context.ContentId.ToString());

                while (pageSiteNode != null)
                {
                    if (pageSiteNode.Title == topLevelTitle && pageSiteNode.ParentNode == pageSiteNode.RootNode)
                    {
                        return true;
                    }

                    pageSiteNode = pageSiteNode.ParentNode as PageSiteNode;
                }
            }

            return false;
        }

        private IWorkflowExecutionDefinition CreateWorkflowForProducts()
        {
            var backendUsersRole = Config.Get<SecurityConfig>().ApplicationRoles[SecurityConstants.AppRoles.BackendUsers];

            // "Approve" is the only level in 1-step approval process.
            // In 2-level approval the steps are "Approve" and "Publish".
            var approvePermission = new WorkflowExecutionPermissionProxy("Approve", backendUsersRole.Id.ToString(), backendUsersRole.Name, WorkflowPrincipalType.Role);
            return new WorkflowDefinitionProxy(
                                                                this.workflowForProductsId,
                                                                "1-step workflow for 'Products'",
                                                                WorkflowType.StandardOneStep,
                                                                true,
                                                                new List<string> { "publishers@example.com" },
                                                                false,
                                                                null,
                                                                false,
                                                                false,
                                                                null,
                                                                new List<IWorkflowExecutionPermission> { approvePermission });
        }

        private IWorkflowExecutionDefinition CreateWorkflowForResources()
        {
            var permissions = new List<IWorkflowExecutionPermission>();

            // Allow everyone in the role "Editors" to approve content.
            var approver1 = Config.Get<SecurityConfig>().ApplicationRoles[SecurityConstants.AppRoles.Designers];

            // In 2-level approval the first step is "Approve"
            permissions.Add(new WorkflowExecutionPermissionProxy("Approve", approver1.Id.ToString(), approver1.Name, WorkflowPrincipalType.Role));

            // Allow user "johnsmith" (if exists) to publish content.
            var publisher1 = UserManager.FindUser("johnsmith");

            // In 2-level approval the second step is "Publish"
            if (publisher1 != null)
                permissions.Add(new WorkflowExecutionPermissionProxy("Publish", publisher1.Id.ToString(), publisher1.UserName, WorkflowPrincipalType.User));

            // Also allow everyone in the role "Designers" to publish content.
            var publisher2 = Config.Get<SecurityConfig>().ApplicationRoles[SecurityConstants.AppRoles.Editors];
            permissions.Add(new WorkflowExecutionPermissionProxy("Publish", publisher2.Id.ToString(), publisher2.Name, WorkflowPrincipalType.Role));

            return new WorkflowDefinitionProxy(
                                                                this.workflowForResourcesId,
                                                                "2-step workflow for 'Resources'",
                                                                WorkflowType.StandardTwoStep,
                                                                true,
                                                                new List<string> { "authors@example.com" },
                                                                true,
                                                                new List<string> { "publishers@example.com" },
                                                                false,
                                                                true,
                                                                null,
                                                                permissions);
        }

        private IWorkflowExecutionDefinition GetWorkflowExecutionDefinitionFromDb(string title)
        {
            return base.GetWorkflowExecutionDefinitionByTitle(title);
        }

        private readonly Guid workflowForProductsId = new Guid("D50640C8-0ECB-4CD0-8C7E-2C258235A7AD");     // Randomly generated.
        private readonly IWorkflowExecutionDefinition workflowForProducts = null;

        private readonly Guid workflowForResourcesId = new Guid("853F0A55-E3D7-4AB2-B590-296D59E175EF");    // Randomly generated.
        private readonly IWorkflowExecutionDefinition workflowForResources = null;

        private readonly IWorkflowExecutionDefinition workflowForDocuments = null;
    }
}
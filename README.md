**Note:** This sample is valid for Sitefinity CMS versions 8.x - 11.0. Sitefintiy CMS 11.1 introduces refactoring and improvements in teh workflow functionality, including 3-level review and approval workflow, as well as ability to specify different workflow definition per page, thus the below sample is no longer necessary, as the functionality comes out of the box. 

For more information about customizing the WorkflowDefinitionResolver class in Sitefinity CMS 11.1 and above, refer to the product documentation here: https://www.progress.com/documentation/sitefinity-cms/111/tutorial-customize-workflow-definitions

Custom workflow definitions in Sitefinity
==========================
Sitefinity's workflow feature allows certain content to require reviewing by a privileged user before publishing. In most cases you should be able to configure everything you need via the admin interface. However, sometimes you may want to enforce more sophisticated rules for determining the way a certain item needs to be approved. For example (but not limited to) you may want pages under certain root category to be approved by one set of approvers, while the rest of the pages to be approved by the default approvers.

So, how do you inject your own code to determine the approval workflow for content items?
With Sitefinity version 8.0, you can do it by descending from **WorkflowDefinitionResolver** and register you new class with the **ObjectFactory**.

## WorkflowDefinitionResolver
This class has 2 overridable methods:
* **IWorkflowExecutionDefinition ResolveWorkflowExecutionDefinition(IWorkflowResolutionContext)** returns a workflow which will be used for approval of the item described by the **IWorkflowResolutionContext** parameter. These are the most common implementations:
	1. Find an existing workflow definition in the database.
	2. Create your own definition using **WorkflowDefinitionProxy** class.
	3. Call base class's method for default behaviour.
* **IWorkflowExecutionDefinition GetWorkflowExecutionDefinition(Guid)** given a Guid, returns workflow definition. If the Guid exists in the database, then you can just call the base class's implementation which will return it correctly. However if you created your own definition with code in **ResolveWorkflowExecutionDefinition**, then you need to return that definition here.

## CustomWorkflowDefinitionResolver example.
The example in this repository is a fully functional class derived from the standard **WorkflowDefinitionResolver**. The example demonstrates how you can override default workflow when a page is located under certain root pages. More specifically it handles content like this:

	1. If the content is not page, then use the default workflow.
	2. If the content is a page under a top-level page called "Products", then a custom generated 1-step workflow is returned.
	3. If the content is a page under a top-level page called "Resources", then a custom generated 2-step workflow is returned.
	4. If the content is a page under a top-level page called "Documents", then it finds in the database a workflow definition called "Workflow for Documents".
	5. Otherwise uses the default workflow provided by the base class.

## Prerequisites
* Sitefinity 8.0 - Sitefinity 11.0

## Build and install
Add to your Global.asax:

In the using section:
```C#
using Telerik.Microsoft.Practices.Unity;
```

and below:
```C#
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Bootstrapper.Initialized += this.Bootstrapper_Initialized;
        }

        void Bootstrapper_Initialized(object sender, Telerik.Sitefinity.Data.ExecutedEventArgs e)
        {
            if (e.CommandName == "Bootstrapped")
            {
                ObjectFactory.Container.RegisterType<Telerik.Sitefinity.Workflow.IWorkflowDefinitionResolver, Telerik.Sitefinity.Samples.CustomWorkflowDefinitionResolver.CustomWorkflowDefinitionResolver>();
            }
        }
    }
```

## API Overview
### IWorkflowResolutionContext
The **IWorkflowResolutionContext** type has several properties, which hold information about the content item which the workflow will be applied to. The following table provides more details about each property.

<table>
	<thead>
		<tr>
			<td><strong>Type</strong></td>
			<td><strong>Property</strong></td>
			<td><strong>Description</strong></td>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><strong>Type</strong></td>
			<td><strong>ContentType</strong></td>
			<td>
				<p>
					.NET type of the item.
				</p>
			</td>
		</tr>
		<tr>
			<td><strong>string</strong></td>
			<td><strong>ContentProviderName</strong></td>
			<td>
				Name of item's provider.
			</td>
		</tr>
		<tr>
			<td><strong>Guid</strong></td>
			<td><strong>ContentId</strong></td>
			<td>
				Item's id.
			</td>
		</tr>
		<tr>
			<td><strong>CultureInfo</strong></td>
			<td><strong>Culture</strong></td>
			<td>
				Content item might be translated into multiple languages, and each translation is approved independently. This parameter tells which translation needs to be approved.
			</td>
		</tr>
		<tr>
			<td><strong>ISite</strong></td>
			<td><strong>Site</strong></td>
			<td>
				The site where content was edited. In some rare cases content items might be shared between multiple sites, so approval in one site will publish it to all.
			</td>
		</tr>
	</tbody>
</table>


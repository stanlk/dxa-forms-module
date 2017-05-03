# DXA Forms Module
The DXA Forms Module is a module for SDL Digital Experience Accelerator (DXA) that provides functionality to easily create web forms in SDL Web. Forms are managed as regular components that can be added to any page. In web applicaton web forms are implemented as classic html forms that are styled with Bootstrap framework.

DXA Forms Module allows to create classic Web Forms as well as AJAX Web Forms that will submit data without refreshing the page. 

So far DXA Forms Module support next field types:
- Text
- Text Area
- DropDown
- Check Box
- Radio Button
- Date

DXA Forms Module comes with built in support for client and server side validation of fields data. You can configure field as required or set more comprehensive validation based on regular expression. Client side validation is implemented with Unobtrusive JQuery Validation library.

How form data will be processed depends on configured form handlers. There are multiple form handlers that are available out of the box, like: EmailHandler, AudienceManagerHandler, etc. Of course you can implement custom form handler that will meet your needs.

# Prerequisites
DXA 1.6 and SDL Web 8/8.5

# Installation
## Content Manager (Step 1)
1. Import DXA Forms Module package into CMS
2. Publish **Forms** Structure Group
3. Publish **Test Category**
4. Republish **Publish Settings** page
## Web Application (Step 2)
1. Unzip DXA Forms into your web application
2. Configure web.config
 * Add next sections and configure smtp server credentials
```
 <system.diagnostics>
    <sources>
      <source name="AudienceManagerLogger" switchName="sourceSwitch">
        <listeners>
          <add name="AudienceManagerTraceListener" />
        </listeners>
      </source>
    </sources>
    <sharedListeners>
      <add name="AudienceManagerTraceListener" initializeData="C:\Temp\logs\am_client.log" rollSizeKb="102400" timestampPattern="yyyy-MM-dd" rollFileExistsBehavior="Increment" rollInterval="Midnight" maxArchivedFiles="0" type="Sdl.AudienceManager.ContentDelivery.Logging.TraceListeners.RollingFlatFileTraceListener, Sdl.AudienceManager.ContentDelivery" />
    </sharedListeners>
    <switches>
      <add name="sourceSwitch" value="Warning" />
    </switches>
  </system.diagnostics>
  <system.net>
    <mailSettings>
      <smtp from="YOUR_EMAIL_ADDRESS">
        <network host="smtp.gmail.com" port="587" userName="YOUR_USER_NAME" password="YOUR_PASSWORD" defaultCredentials="false" enableSsl="true" />
      </smtp>
    </mailSettings>
  </system.net>
```

3. Enable **UnotrusiveJavaScrip** in web.config
- Add next line to your **appSettings**
```
<add key="UnobtrusiveJavaScriptEnabled" value="true" />
```
4. Add JavaScript and CSS references to your Layout.cs
- Insert next code after **@RenderBody()**
```
    <script src="~/Scripts/jquery-1.8.0.min.js"></script>
    <script src="~/Scripts/jquery.validate.min.js"></script>
    <script src="~/Scripts/jquery.unobtrusive-ajax.min.js"></script>
    <script src="~/Scripts/jquery.validate.unobtrusive.min.js"></script>
    <script src="~/Scripts/bootstrap-datepicker.min.js"></script>
    <link href="~/Content/bootstrap-datepicker.min.css" rel="stylesheet" />
    <script type="text/javascript">
        (function ($) {

            $.validator.unobtrusive.adapters.add("mandatory", function (options) {
                options.rules["required"] = true;
                if (options.message) {
                    options.messages["required"] = options.message;
                }
            });
        }(jQuery));
    </script>
```
5. If you will use Audience Manager Handler make sure that cd_audience_manager_conf.xml exist in your *config* folder

## Testing (Step 3)
1. Run your applicatoin
2. Navigatoin to **/forms/form-test**
3. You should see working test form

# License


Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and limitations under the License.

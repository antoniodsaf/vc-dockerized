# Virtocommerce

## Getting Started

This repository is unifying the entire stack of virtocommerce (w/ their dependencies) using docker, it contains [vc-platform](https://github.com/VirtoCommerce/vc-platform) and [vc-storefront](https://github.com/VirtoCommerce/vc-storefront). 
The dependencies and technologies included are:

- **dotnet (core)** - software platform (it's used to implement/deploy the backend stuff)
- **MS SQL Server** - relational database (to keep the commerce data)
- **Elasticsearch** - engine search (indexing the catalog data)
- **Redis** - caching data
- **vuejs** - javascript framework (it's used on storefront)

## Repository Path Structure

This repository have the following path structure:

```
root
|-- storefront - folder that keeps all storefront folders
|   |-- Themes - folder structure used by virtocommerce
|   |   |-- B2B-store - folder that refers to 'B2B-Store' store (we can have multiple)
|   |   |   |-- vc-theme-b2b-vue - it refers to vc-theme-b2b-vue (sample theme from virtocommerce)
|   |   |-- sample - that refers to 'sample' store
|-- platform-modules - folder that contains all platform modules (backend part) implemented by us
|   |-- vc-module-my-loyalty - module folder containing the integration with CPQ (sample built to expose a graphql query that is consumed by storefront)
```

## How to run locally (tested on ubuntu 22.04 LTS)

First step, create a network called `nat`, should be executed only once (only at the first time):
> ``` docker network create nat ```

After the network has been created,* should execute:
> ```docker compose up --build -d```

This above command, will build the dockerfiles (platform and storefront) and then expose the services (sqlserver, elastic, redis, platform, and storefront).

**Note:** The platform and storefront are using a specific version, if you want to change, do this changing the `.env` file.

```
PLATFORM_VERSION=3.815.0
STOREFRONT_VERSION=6.40.0-master-0380cf97
 ```
if you also want to change any env variable (like passwords, volume folders), please consider to look at the `.env` file, below has other useful `env variables` (we can keep as it is):

 ```bash
#CMS_CONTENT_VOLUME: folder shared by storefront and platform, this keeps the CMS files/assets responsible to expose the storefront view.

#MODULES_VOLUME: folder used in platform, this one keeps all modules registered by the vc-platform.

#ASSETS_VOLUME: folder used in platform to keep assets files (such as product images).

(by default is using this root path to keep the volumes)
 ```

After the command `docker compose up --build -d` has been executed:
- The **platform** is exposed under the `8090 port`
- The **storefront** is exposed under the `8091 port`

At the first time, it takes more time because it will be necessary to set up the environment. (to do the setup you should access http://localhost:8090 and inform **user: admin and password: store**)


## How to set up (and update) the storefront

Basically we need the `assets folder` (built) and the `vc-theme-b2b-vue theme activated` (associated to the store to proceed), so inside the storefront theme folder (for example `storefront/Themes/B2B-store/vc-theme-b2b-vue`), you should build:

1. `yarn install` (this will install the deps)
2. `yarn build` (this will create the assets folder, this one has the files responsible to built/render the storefront)

**These above commands will update the storefront.**

**Note:** yarn builds the theme by going in same directory make sure you have the same node version (19 > version >= 18) yarn version, you can check it from package.json file.

1. After built the theme (the above steps), you should access the platform http://localhost:8090, and click on **"Content"** (menu page or access directly using the url http://localhost:8090/#!/workspace/content) to to enable it.
2. Click on **"B2B-store" and "Themes"**, and then click on right button on **'vc-theme-b2b-vue'** choosing the option **"Set Active"**.
3. After the 'vc-theme-b2b-vue' activation maybe you'll need to **restart** (you can do this `via docker` or via `UI platform`)

**The above steps 1-3, should be made only the first time** (when you're configuring the theme, so after the theme activation, you'll just need to run the `yarn build` responsible to generate the `assets files`).


So, finally now you're able to access the storefront via http://localhost:8091


## How to deploy a platform module

1- Creating it from template, using `vc-cli-module-template` library:

```bash
dotnet new install VirtoCommerce.Module.Template
```

> Command to create a new module from template:

```bash
dotnet new vc-module --ModuleName CustomerReviews --Author "Jon Doe" --CompanyName VirtoCommerce #need to inform ModuleName, Author and CompanyName
```

> Command to create a new XAPI-module from template:

```bash
dotnet new vc-module-xapi --ModuleName QuoteExperienceApi --Author "Jon Doe" --CompanyName VirtoCommerce #need to inform ModuleName, Author and CompanyName
```




Access the site https://github.com/VirtoCommerce/vc-cli-module-template for more details on how to use the template generator.

---

2- After the module (template) has been created and all implementation has been done, follow the below steps to deploy the module:


Under the module (for example `platform-modules/vc-module-my-loyalty`), execute the below commands:

1. `dotnet build` (this command compile the module)
2. create a new folder to receive the new module on `MODULES_VOLUME` folder
3. go to the folder `src/*.Web` (in this case `src/ObjectEdge.MyLoyalty.Web/`)
4. **copy all files hereand paste on the folder** that you created in step 2.
5. restart the platform (you can do this using the UI [http://localhost:8090] or via docker)




for more vc-modules detailing, please click on the below links from virtocommerce documentation:

[Deploy from source code](https://docs2.govirto.com/developer-guide/deploy-module-from-source-code/)

[Create a new module from Scratch](https://docs2.govirto.com/developer-guide/create-new-module-advanced/)

[How to debug using docker](https://docs2.govirto.com/developer-guide/modules-development-via-docker/#debugging-the-module)


## How to run the frontend (debug)

under the storefront theme folder (for example `storefront/Themes/B2B-store/vc-theme-b2b-vue`):

1. `yarn install` (install de deps)
2. change the file `.env`, adding the port *8091* on *APP_BACKEND_URL var*, so the final should be `APP_BACKEND_URL=http://localhost:8091`
3. `yarn dev`
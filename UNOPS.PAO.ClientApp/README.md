# UnopsPAOClient

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 19.0.5.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.


## Front End Principles

### CSS

1. **Stick to tailwind / [Sakai template](https://sakai.primeng.org/)**. Use divs and spans as much as possible.
2. Use **gap-2, gap-4 or gap-8** for spacing between elements. Avoid using margin or padding classes.
3. For the layout : **layout (page level) / p-panel / elements**. Avoid double frames.

### Angular (suggesting)

#### 1. Structure of components :

Suggestion of structure for the components :

```
contact
+-- new
    +-- contact-new.component.html
    +-- contact-new.component.ts
+-- list
    +-- contact-list.component.html
    +-- contact-list.component.ts
+-- contact.component.html
+-- contact.component.ts
```

The standard is to use snake case for the file names.

#### 2. Managing data :
  - **contact.service.ts** : for the API calls
  - **contact.data.ts** : for the data (not standard)
  - **contact.model.ts** : for the interface

#### 3. Pagination

**pagination-url.service.ts** : for the pagination in the URL.

I could be nice to have an abstract class for DataServices that would handle the pagination.

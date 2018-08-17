import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';

import { MsalModule, MsalGuard, MsalInterceptor, MsalService } from '@azure/msal-angular';
import { LogLevel } from 'msal';

// Logger callback for MSAL
export function loggerCallback(logLevel, message, piiEnabled) {
    console.log(message);
}

// Note - locally we're using http for Azure Functions.  This is needed for the interceptor to attach tokens when making calls to the Function API.
export const protectedResourceMap: Map<string, Array<string>> = new Map<string, Array<string>>();
protectedResourceMap.set("http://localhost:7071/api/", ["https://letsbuildit.onmicrosoft.com/demoapi/demo.read"]);

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    MsalModule.forRoot({
      authority: "https://login.microsoftonline.com/tfp/letsbuildit.onmicrosoft.com/B2C_1_SiUpIn",
      consentScopes: ["profile", "https://letsbuildit.onmicrosoft.com/demoapi/demo.read"],
      clientID: "c17514de-1ccc-4bca-b4c7-92d1f8eb506a",
      popUp: true,
      protectedResourceMap: protectedResourceMap,
      postLogoutRedirectUri: "https://localhost:44356/",
      logger: loggerCallback,
      level: LogLevel.Verbose
    }),
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data', component: FetchDataComponent, canActivate: [MsalGuard] },
    ])
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

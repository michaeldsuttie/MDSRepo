import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
//import { FetchDataComponent } from './components/fetchdata/fetchdata.component';
import { GetProjectsComponent } from './components/getprojects/getprojects.component';
import { GetProjectDataComponent } from './components/getprojectdata/getprojectdata.component';
//import { CounterComponent } from './components/counter/counter.component';
//import { DropDownMenuComponent } from './components/dropdownmenu/dropdownmenu.component';
import { GetProjectBillingSummaryComponent } from './components/getprojectbillingsummary/getprojectbillingsummary.component';

import { ObjectToArrayPipe } from './components/getprojectbillingsummary/getprojectbillingsummary.component';

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        ObjectToArrayPipe,
        AppComponent,
        NavMenuComponent,
        //CounterComponent,
        //FetchDataComponent,
        GetProjectsComponent,
        GetProjectDataComponent,
        GetProjectBillingSummaryComponent,
        HomeComponent
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            //{ path: 'counter', component: CounterComponent },
            //{ path: 'fetch-data', component: FetchDataComponent },
            { path: 'get-projects', component: GetProjectsComponent },
            { path: 'get-project-data', component: GetProjectDataComponent },
            { path: 'get-project-billing-summary', component: GetProjectBillingSummaryComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModule {
}

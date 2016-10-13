import { Component } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'get-project-data',
    template: require('./getprojectdata.component.html')
})
export class GetProjectDataComponent {
    public entries: TogglTimeEntry[];

    constructor(http: Http) {
        http.get('/api/TogglData/GetProjectEntries').subscribe(result => {
            this.entries = result.json();
        });
    }
}

interface TogglTimeEntry {
    //dateFormatted: string;
    //temperatureC: number;
    //temperatureF: number;
    //summary: string;
}

import { Component } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'get-projects',
    template: require('./getprojects.component.html')
})
export class GetProjectsComponent {
    public projects: ProjectDetail[];

    constructor(http: Http) {
        http.get('/api/TogglData/GetProjects').subscribe(result => {
            this.projects = result.json();
        });
    }
}

interface ProjectDetail {
    id: number;
    name: string;
    client: string;
}

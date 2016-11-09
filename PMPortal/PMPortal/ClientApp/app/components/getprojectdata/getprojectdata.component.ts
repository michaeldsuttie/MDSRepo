import { Component } from '@angular/core';
import { Http } from '@angular/http';



@Component({
    selector: 'get-project-data',
    template: require('./getprojectdata.component.html'),
})
export class GetProjectDataComponent {
    public http: Http;
    public entries: TogglTimeEntry[];
    public projects: Project[];
    public selectedProject: Project;
    //public selectedProjectString: string;
    public selectedProjectName: string;
    public selectedProjectNumber: string;
    public since: Date;
    public until: Date;


    onSelect(project: Project): void {
        this.selectedProject = project;
        //this.selectedProjectNumber = project.id;
        this.getProjectEntries(this.selectedProjectNumber, this.since, this.until);
    }

    //onSelect(selectedProjectString: string): void {
    //    this.selectedProject = selectedProjectString;
    //    this.selectedProjectString = selectedProjectString;
    //    this.getProjectEntries(this.selectedProjectNumber);
    //}

    //onChange(value: string): void {
    //    this.selectedProjectNumber = value;
    //    this.getProjectEntries(this.selectedProjectNumber);
    //}

    getProjectEntries(projectNumber: string, since: Date, until: Date): void {

        var entryRequest = '/api/TogglData/GetProjectEntries' + '?projectnumber=' + projectNumber + '&since=2016-10-06' + '&until=2016-10-08';
        this.http.get(entryRequest).subscribe(result => {
            this.entries = result.json();
        });
    }

    constructor(http: Http) {
        this.http = http;
        http.get('/api/TogglData/GetProjects').subscribe(result => {
            this.projects = result.json();
        });

        this.selectedProjectNumber = "16081";
        //this.since = new Date("2016-10-06");
        //this.until = new Date("2016-10-08");
        this.getProjectEntries(this.selectedProjectNumber, this.since, this.until);
    }
}

interface TogglTimeEntry {
    //dateFormatted: string;
    //temperatureC: number;
    //temperatureF: number;
    //summary: string;
}
interface Project {
    id: number;
    name: string;
    client: string;
}
import { Pipe, PipeTransform, Component } from '@angular/core';
import { Http } from '@angular/http';

@Pipe({ name: "objectToArray" })
export class ObjectToArrayPipe implements PipeTransform {
    transform(value, args: string[]): any {
        let keys = [];
        for (let key in value) {
            keys.push({ key: key, value: value[key] });
        }
        return keys;
    }
}

@Component({
    selector: 'get-project-billing-summary',
    template: require('./getprojectbillingsummary.component.html'),
})
export class GetProjectBillingSummaryComponent {
    public http: Http;
    public entries: TogglTimeEntry[];
    public summary: TogglProjectBillingSummary;
    //public summaryKeys;
    //public taskSummaries: TogglTaskSummary[];
    public projects: Project[];
    public selectedProject: Project;
    public selectedProjectName: string;
    public selectedProjectNumber: string;
    public since: Date;
    public until: Date;
    public sinceString: string;
    public untilString: string;

    constructor(http: Http) {
        this.http = http;
        http.get('/api/TogglData/GetProjects').subscribe(result => {
            this.projects = result.json();
        });

        //this.selectedProjectNumber = "16081";
        //this.since = new Date("2016-10-06");
        //this.until = new Date("2016-10-08");
        //this.summaryKeys = Object.keys(this.summary);

        this.getProjectBilingSummary(this.selectedProjectNumber, this.since, this.until);

    }

    //onSelect(project: Project): void {
    //    this.selectedProject = project;
    //    this.getProjectBilingSummary(this.selectedProjectNumber, this.since, this.until);
    //}

    getProjectBilingSummary(projectNumber: string, since: Date, until: Date): void {
        this.selectedProjectNumber = '16081';
        this.sinceString = '2016-10-06';
        this.untilString = '2016-10-08';
        var billingSummaryRequest = '/api/TogglData/GetProjectBillingSummary' + '?projectnumber=' + this.selectedProjectNumber + '&since=' + this.sinceString + '&until=' + this.untilString;
        this.http.get(billingSummaryRequest).subscribe(result => {
            this.summary = result.json();
        });
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

interface TogglProjectBillingSummary {

}

interface TogglTaskSummary {

}
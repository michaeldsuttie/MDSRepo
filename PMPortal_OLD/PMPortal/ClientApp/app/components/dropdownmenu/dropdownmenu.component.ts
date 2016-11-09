//import { Component } from 'angular2/core';
//import { CORE_DIRECTIVES } from 'angular2/common';
//import { Dropdown } from './dropdown.directive';
//import { DropdownMenu } from './dropdown-menu.directive';
//import { DropdownToggle } from './dropdown-toggle.directive';
///*Angular 2 Dropdown Menu*/
//@Component({
//    selector: 'my-app',
//    template: `

//                    <div (click)="$event.preventDefault()">
//                      <!-- Angular 2 Simple Dropdown Menu -->
//                      <h4>Angular 2 Simple Dropdown Menu</h4>
//                      <span dropdown>
//                        <a href id="angular-simple-dropdown" dropdownToggle>
//                                angular 2 simple dropdown menu
//                        </a>
//                        <ul class="dropdown-menu" aria-labelledby="angular-simple-dropdown">
//                          <li *ngFor="#item of dropDownItemsExample">
//                            <a class="dropdown-item" href="#">{{item}}</a>
//                          </li>
//                        </ul>
//                      </span>
//                      <hr>
//                      <!-- Angular 2 Dropdown Menu with Enable/Disable mode and Events (external click) -->
//                      <h4>Angular 2 Dropdown Menu with Enable/Disable mode and Events (external click)</h4>
//                      <div>
//                        <button type="button" class="btn btn-warning btn-sm" (click)="dropdownMenu($event)">Dropdown Menu (Toggle)
//                        </button>
//                        <button type="button" class="btn btn-danger btn-sm" (click)="disabledMenu = !disabledMenu">Enable/Disable Dropdown Menu</button>
//                      </div>
//                      <div class="btn-group" dropdown [(isOpen)]="status.isopen">
//                        <button id="dropdown-list" type="button" class="btn btn-default" dropdownToggle [disabled]="disabledMenu">
//                          Button dropdown <span class="caret"></span>
//                        </button>
//                        <ul class="dropdown-menu" role="menu" aria-labelledby="dropdown-list">
//                          <li role="menuitem"><a class="dropdown-item" href="#">Audi</a></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">BMW</a></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">Mercedes</a></li>
//                          <li class="divider dropdown-divider"></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">Maserati</a></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">Porsche</a></li>
//                        </ul>
//                      </div>

//                      <hr>

//                      <!-- Angular 2 Dropdown Menu with Keyboard Accessibility -->
//                      <h4>Angular 2 Dropdown Menu with Keyboard Accessibility</h4>
//                      <div class="btn-group" dropdown keyboardNav="true">
//                        <button id="dropdown-keyboard-access" type="button" class="btn btn-info" dropdownToggle>
//                          Dropdown with Keyboard Accessibility (Up and Down) <span class="caret"></span>
//                        </button>
//                        <ul class="dropdown-menu" role="menu" aria-labelledby="dropdown-keyboard-access">
//                          <li role="menuitem"><a class="dropdown-item" href="#">BMW Serie 1</a></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">BMW Serie 5</a></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">BMW Serie 3</a></li>
//                          <li class="divider dropdown-divider"></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">Porsche</a></li>
//                          <li role="menuitem"><a class="dropdown-item" href="#">Audi New</a></li>
//                        </ul>
//                      </div>
//                    </div>
//               `,
//    directives: [Dropdown, DropdownMenu, DropdownToggle, CORE_DIRECTIVES],
//})
//export class DropDownMenuComponent {

//    private disabledMenu: boolean = false;

//    private status: { isopen: boolean } = { isopen: false };

//    private dropDownItemsExample: Array<string> = ['BMW Serie 1', 'BMW Serie 2', 'BMW Serie 3', 'BMW Serie 4'];

//    private dropdownMenu($event: MouseEvent): void {
//        $event.preventDefault();
//        $event.stopPropagation();
//        this.status.isopen = !this.status.isopen;
//    }

//}
Feature: Login
  In order to access tracking data
  As a carrier provider
  I want to be able to log in with username and password

  Background:
    Given the application base URL is "https://site.com"

  Scenario: Present login screen for anonymous user
    Given I am not logged in
    When I navigate to "/any-page"
    Then I should be presented with a login screen

  Scenario: Successful login with valid credentials
    Given valid user credentials are registered
    And I'm on the login screen
    When I enter a valid username and password and submit
    Then I am logged in successfully
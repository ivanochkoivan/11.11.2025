Feature: Login on tracking site

  Scenario: Anonymous user is shown login when navigating anywhere
    Given I am not logged in
    When I navigate to "/anypage"
    Then I should see the login screen

  Scenario: Valid user can login successfully
    Given I'm on the login screen
    When I enter a valid username and password and submit
    Then I am logged in successfully
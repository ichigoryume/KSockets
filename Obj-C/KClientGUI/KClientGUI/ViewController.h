//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface ViewController : UIViewController

@property (retain, nonatomic) IBOutlet UIView *scrollView;

@property (retain, nonatomic) IBOutlet UISegmentedControl *protocolSegmentControl;
@property (retain, nonatomic) IBOutlet UIView *protocolSettingSliderView;

@property (retain, nonatomic) IBOutlet UITextField *ipAddressTextField;
@property (retain, nonatomic) IBOutlet UITextField *portTextFiled;
@property (retain, nonatomic) IBOutlet UITextField *messageSizeTextField;
@property (retain, nonatomic) IBOutlet UITextField *terminationStringTextField;
@property (retain, nonatomic) IBOutlet UITextField *messageTextBox;
@property (retain, nonatomic) IBOutlet UITextView *logTextView;

- (IBAction)ipAddressTextFieldDidEndOnExit:(id)sender;
- (IBAction)protocolSegmentValueChanged:(id)sender;
- (IBAction)portTextFieldDidEndOnExit:(id)sender;
- (IBAction)messageSizeTextFieldDidEndOnExit:(id)sender;
- (IBAction)terminationStringTextFiledDidEndOnExit:(id)sender;
- (IBAction)messageTextFiledEditingDidBegin:(id)sender;
- (IBAction)messageTextFieldDidEndOnExit:(id)sender;

- (IBAction)connectButtonTouchUpInside:(UIButton *)sender;
- (IBAction)randomButtonTouchUpInside:(UIButton *)sender;
- (IBAction)sendButtonTouchUpInside:(UIButton *)sender;

@end
